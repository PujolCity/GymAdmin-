using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;
using GymAdmin.Infrastructure.Paths.FolderConfig;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace GymAdmin.Infrastructure.Backup.DailyBackup;

public class BackupRestoreService : IBackupRestoreService
{
    private readonly IAppPaths _appPaths;
    private readonly ILogger<BackupRestoreService> _logger;

    public BackupRestoreService(IAppPaths appPaths,
        ILogger<BackupRestoreService> logger)
    {
        _appPaths = appPaths;
        _logger = logger;
    }

    public async Task<Result> RestoreDailyBackupAsync(
        string zipFilePath,
        bool restoreLogs,
        CancellationToken ct = default)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gymadmin-restore-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        string? rollbackPath = null;
        var dbReplaced = false;
        var rollbackNeeded = false;

        try
        {
            _logger.LogInformation("Iniciando restore desde {ZipFile}", zipFilePath);

            var validationResult = ValidateRestoreRequest(zipFilePath, _appPaths.DbFile);
            if (!validationResult.IsSuccess)
            {
                _logger.LogWarning("Validación de restore fallida: {Errors}", string.Join(" | ", validationResult.Errors));
                return validationResult;
            }

            var currentDbPath = _appPaths.DbFile;
            var dbFileName = Path.GetFileName(currentDbPath);

            _logger.LogInformation("Base actual detectada en {CurrentDbPath}", currentDbPath);
            _logger.LogInformation("Extrayendo backup en carpeta temporal {TempDir}", tempDir);

            var extractResult = ExtractBackup(zipFilePath, tempDir);
            if (!extractResult.IsSuccess)
                return extractResult;

            var extractedDbPath = Path.Combine(tempDir, dbFileName);

            if (!File.Exists(extractedDbPath))
            {
                _logger.LogWarning("El backup no contiene el archivo esperado {DbFileName}", dbFileName);
                return Result.Fail($"El backup no contiene el archivo de base esperado: {dbFileName}");
            }

            rollbackPath = BuildRollbackPath(currentDbPath);
            _logger.LogInformation("Generando backup previo al restore en {RollbackPath}", rollbackPath);

            var backupResult = await BackupDatabaseAsync(currentDbPath, rollbackPath, ct);
            if (!backupResult.IsSuccess)
                return backupResult;

            rollbackNeeded = true;

            ReleaseSqliteHandles();

            _logger.LogInformation("Reemplazando DB actual con la del backup");
            var replaceDbResult = ReplaceFile(extractedDbPath, currentDbPath);
            if (!replaceDbResult.IsSuccess)
                return replaceDbResult;

            dbReplaced = true;
            _logger.LogInformation("DB reemplazada correctamente");

            // Simulación de prueba:
             throw new Exception("Falla simulada después de reemplazar la DB");

            if (restoreLogs)
            {
                var extractedLogsPath = Path.Combine(tempDir, "Logs");

                if (Directory.Exists(extractedLogsPath))
                {
                    _logger.LogInformation("Restaurando logs desde {ExtractedLogsPath} hacia {LogsDir}", extractedLogsPath, _appPaths.LogsDir);

                    var restoreLogsResult = RestoreDirectory(extractedLogsPath, _appPaths.LogsDir);
                    if (!restoreLogsResult.IsSuccess)
                    {
                        _logger.LogWarning("Falló la restauración de logs después de haber reemplazado la DB. Se intentará rollback.");
                        var rollbackResult = TryRestoreRollback(rollbackPath, currentDbPath);

                        if (rollbackResult.IsSuccess)
                        {
                            rollbackNeeded = false;
                            return Result.Fail("La restauración falló al restaurar los logs, pero la base original fue recuperada automáticamente.");
                        }

                        return Result.Fail(
                            "La restauración falló al restaurar los logs y además no se pudo recuperar automáticamente la base anterior. " +
                            $"Backup previo disponible en: {rollbackPath}");
                    }
                }
                else
                {
                    _logger.LogInformation("El backup no contiene carpeta Logs. Se omite restauración de logs.");
                }
            }
            else
            {
                _logger.LogInformation("El usuario eligió no restaurar logs.");
            }

            _logger.LogInformation("Restore finalizado correctamente");

            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Restore cancelado por token de cancelación.");

            if (dbReplaced)
            {
                var rollbackResult = TryRestoreRollback(rollbackPath, _appPaths.DbFile);
                if (rollbackResult.IsSuccess)
                {
                    rollbackNeeded = false;
                    return Result.Fail("La restauración fue cancelada, pero la base original fue recuperada automáticamente.");
                }

                return Result.Fail(
                    "La restauración fue cancelada y además no se pudo recuperar automáticamente la base anterior. " +
                    $"Backup previo disponible en: {rollbackPath}");
            }

            return Result.Fail("La restauración fue cancelada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante restore de backup.");

            if (dbReplaced)
            {
                var rollbackResult = TryRestoreRollback(rollbackPath, _appPaths.DbFile);
                if (rollbackResult.IsSuccess)
                {
                    rollbackNeeded = false;
                    return Result.Fail("La restauración falló, pero la base original fue recuperada automáticamente.");
                }

                return Result.Fail(
                    "La restauración falló y además no se pudo recuperar automáticamente la base anterior. " +
                    $"Backup previo disponible en: {rollbackPath}");
            }

            return Result.Fail($"Error inesperado durante restore de backup: {ex.Message}");
        }
        finally
        {
            TryDeleteTempDirectory(tempDir);

            if (!rollbackNeeded)
            {
                DeleteRollbackIfExists(rollbackPath);
            }
            else
            {
                _logger.LogWarning("Se conserva el backup previo para recuperación manual: {RollbackPath}", rollbackPath);
            }
        }
    }

    private Result ValidateRestoreRequest(string zipFilePath, string currentDbPath)
    {
        if (string.IsNullOrWhiteSpace(zipFilePath))
            return Result.Fail("La ruta del backup es obligatoria.");

        if (!File.Exists(zipFilePath))
            return Result.Fail("No se encontró el archivo ZIP del backup.");

        if (string.IsNullOrWhiteSpace(currentDbPath))
            return Result.Fail("No se pudo determinar la ruta de la base actual.");

        if (!File.Exists(currentDbPath))
            return Result.Fail($"No existe la base actual en la ruta: {currentDbPath}");

        return Result.Ok();
    }

    private Result ExtractBackup(string zipFilePath, string tempDir)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipFilePath, tempDir);
            _logger.LogInformation("Backup extraído correctamente en {TempDir}", tempDir);
            return Result.Ok();
        }
        catch (InvalidDataException ex)
        {
            _logger.LogError(ex, "El archivo ZIP está corrupto o no es válido: {ZipFilePath}", zipFilePath);
            return Result.Fail("El archivo ZIP está corrupto o no tiene un formato válido.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo extraer el ZIP de backup: {ZipFilePath}", zipFilePath);
            return Result.Fail($"No se pudo extraer el ZIP de backup: {ex.Message}");
        }
    }

    private Result TryRestoreRollback(string? rollbackPath, string currentDbPath)
    {
        if (string.IsNullOrWhiteSpace(rollbackPath))
        {
            _logger.LogWarning("No hay ruta de rollback disponible.");
            return Result.Fail("No hay ruta de rollback disponible.");
        }

        if (!File.Exists(rollbackPath))
        {
            _logger.LogWarning("No existe el archivo de rollback en {RollbackPath}", rollbackPath);
            return Result.Fail("No existe el archivo de rollback.");
        }

        try
        {
            _logger.LogWarning("Intentando rollback automático de la base desde {RollbackPath}", rollbackPath);

            ReleaseSqliteHandles();

            var replaceResult = ReplaceFile(rollbackPath, currentDbPath);
            if (!replaceResult.IsSuccess)
            {
                _logger.LogError("Falló el reemplazo durante rollback: {Errors}", string.Join(" | ", replaceResult.Errors));
                return replaceResult;
            }

            _logger.LogWarning("Rollback automático realizado correctamente.");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló también el rollback automático de la base.");
            return Result.Fail($"Falló también el rollback automático: {ex.Message}");
        }
    }

    private void DeleteRollbackIfExists(string? rollbackPath)
    {
        if (string.IsNullOrWhiteSpace(rollbackPath))
            return;

        try
        {
            if (File.Exists(rollbackPath))
            {
                File.Delete(rollbackPath);
                _logger.LogInformation("Se eliminó el backup previo de restore: {RollbackPath}", rollbackPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar el backup previo de restore: {RollbackPath}", rollbackPath);
        }
    }

    private void TryDeleteTempDirectory(string tempDir)
    {
        try
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
                _logger.LogDebug("Carpeta temporal eliminada: {TempDir}", tempDir);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar carpeta temporal: {TempDir}", tempDir);
        }
    }

    private static void ReleaseSqliteHandles()
    {
        SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private static string BuildRollbackPath(string currentDbPath)
    {
        var dir = Path.GetDirectoryName(currentDbPath)
            ?? throw new InvalidOperationException("No se pudo determinar el directorio de la base actual.");

        var file = $"pre-restore-{DateTime.Now:yyyyMMdd-HHmmss}.db";
        return Path.Combine(dir, file);
    }

    private async Task<Result> BackupDatabaseAsync(string sourcePath, string destinationPath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            return Result.Fail("La ruta origen de la base es inválida.");

        if (!File.Exists(sourcePath))
            return Result.Fail($"No se encontró la base origen para generar el rollback previo: {sourcePath}");

        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (string.IsNullOrWhiteSpace(destinationDirectory))
            return Result.Fail("No se pudo determinar el directorio del rollback.");

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            var sourceConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = sourcePath,
                Mode = SqliteOpenMode.ReadOnly,
                Pooling = false
            }.ToString();

            var destinationConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = destinationPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false
            }.ToString();

            await using var sourceConnection = new SqliteConnection(sourceConnectionString);
            await using var destinationConnection = new SqliteConnection(destinationConnectionString);

            await sourceConnection.OpenAsync(ct);
            await destinationConnection.OpenAsync(ct);

            sourceConnection.BackupDatabase(destinationConnection);

            await destinationConnection.CloseAsync();
            await sourceConnection.CloseAsync();

            SqliteConnection.ClearAllPools();

            _logger.LogInformation("Backup previo generado correctamente en {DestinationPath}", destinationPath);
            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Se canceló la generación del backup previo.");
            return Result.Fail("Se canceló la generación del backup previo.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando backup previo en {DestinationPath}", destinationPath);
            return Result.Fail($"Error generando backup previo: {ex.Message}");
        }
    }

    private Result ReplaceFile(string source, string destination)
    {
        if (string.IsNullOrWhiteSpace(source))
            return Result.Fail("La ruta origen es obligatoria.");

        if (string.IsNullOrWhiteSpace(destination))
            return Result.Fail("La ruta destino es obligatoria.");

        if (!File.Exists(source))
            return Result.Fail($"No se encontró el archivo origen para reemplazo: {source}");

        var dir = Path.GetDirectoryName(destination);
        if (string.IsNullOrWhiteSpace(dir))
            return Result.Fail("No se pudo determinar el directorio destino.");

        try
        {
            Directory.CreateDirectory(dir);

            if (File.Exists(destination))
            {
                File.Delete(destination);
                _logger.LogDebug("Archivo destino eliminado antes de reemplazar: {Destination}", destination);
            }

            File.Copy(source, destination, overwrite: false);
            _logger.LogInformation("Archivo reemplazado correctamente desde {Source} hacia {Destination}", source, destination);

            var walPath = destination + "-wal";
            var shmPath = destination + "-shm";

            if (File.Exists(walPath))
            {
                File.Delete(walPath);
                _logger.LogDebug("Archivo WAL eliminado: {WalPath}", walPath);
            }

            if (File.Exists(shmPath))
            {
                File.Delete(shmPath);
                _logger.LogDebug("Archivo SHM eliminado: {ShmPath}", shmPath);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reemplazando archivo desde {Source} hacia {Destination}", source, destination);
            return Result.Fail($"Error reemplazando archivo: {ex.Message}");
        }
    }

    private Result RestoreDirectory(string sourceDir, string targetDir)
    {
        if (string.IsNullOrWhiteSpace(sourceDir))
            return Result.Fail("La carpeta origen de logs es inválida.");

        if (!Directory.Exists(sourceDir))
            return Result.Fail($"La carpeta origen de logs no existe: {sourceDir}");

        if (string.IsNullOrWhiteSpace(targetDir))
            return Result.Fail("La carpeta destino de logs es inválida.");

        try
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
                _logger.LogDebug("Log restaurado: {File}", destFile);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(targetDir, Path.GetFileName(dir));
                var result = RestoreDirectory(dir, destSubDir);
                if (!result.IsSuccess)
                    return result;
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restaurando carpeta de logs desde {SourceDir} hacia {TargetDir}", sourceDir, targetDir);
            return Result.Fail($"Error restaurando logs: {ex.Message}");
        }
    }
}