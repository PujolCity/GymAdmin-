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

    public async Task<Result> RestoreDailyBackupAsync(string zipFilePath, bool restoreLogs, CancellationToken ct = default)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gymadmin-restore-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            _logger.LogInformation("Iniciando restore desde {ZipFile}", zipFilePath);

            // 1. Extraer ZIP
            ZipFile.ExtractToDirectory(zipFilePath, tempDir);

            var extractedDbPath = Path.Combine(tempDir, "gymadmin.db");
            if (!File.Exists(extractedDbPath))
                return Result.Fail("El backup no contiene gymadmin.db");

            var currentDbPath = _appPaths.DbFile;

            // 2. Backup de seguridad antes de pisar
            var rollbackPath = BuildRollbackPath(currentDbPath);
            _logger.LogInformation("Generando backup previo en {RollbackPath}", rollbackPath);

            await BackupDatabaseAsync(currentDbPath, rollbackPath, ct);

            // 3. Liberar SQLite
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // 4. Reemplazar DB
            _logger.LogInformation("Reemplazando DB actual con la del backup");

            ReplaceFile(extractedDbPath, currentDbPath);

            // 5. Restaurar logs (opcional)
            if (restoreLogs)
            {
                var extractedLogsPath = Path.Combine(tempDir, "Logs");

                if (Directory.Exists(extractedLogsPath))
                {
                    _logger.LogInformation("Restaurando logs");

                    RestoreDirectory(extractedLogsPath, _appPaths.LogsDir);
                }
            }

            _logger.LogInformation("Restore finalizado correctamente");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante restore de backup");
            throw;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar carpeta temporal");
            }
        }
    }

    private static string BuildRollbackPath(string currentDbPath)
    {
        var dir = Path.GetDirectoryName(currentDbPath)!;
        var file = $"pre-restore-{DateTime.Now:yyyyMMdd-HHmmss}.db";
        return Path.Combine(dir, file);
    }

    private static async Task BackupDatabaseAsync(string sourcePath, string destinationPath, CancellationToken ct)
    {
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
    }

    private static void ReplaceFile(string source, string destination)
    {
        var dir = Path.GetDirectoryName(destination)!;
        Directory.CreateDirectory(dir);

        if (File.Exists(destination))
        {
            File.Delete(destination);
        }

        File.Copy(source, destination, overwrite: false);
    }

    private static void RestoreDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(targetDir, Path.GetFileName(dir));
            RestoreDirectory(dir, destSubDir);
        }
    }
}
