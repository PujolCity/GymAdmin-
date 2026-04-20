using GymAdmin.Infrastructure.Config.Options;
using GymAdmin.Infrastructure.Data;
using GymAdmin.Infrastructure.Paths.BackupPaths;
using GymAdmin.Infrastructure.Paths.FolderConfig;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;

namespace GymAdmin.Infrastructure.Backup.DailyBackup;

public class BackupService : IBackupService
{
    private readonly IAppPaths _appPaths;
    private readonly IBackupPaths _backupPaths;
    private readonly BackupConfig _config;
    private readonly GymAdminDbContext _context;
    private readonly ILogger<BackupService> _logger;

    public BackupService(ILogger<BackupService> logger,
        GymAdminDbContext context,
        IOptions<BackupConfig> config,
        IBackupPaths backupPaths,
        IAppPaths appPaths)
    {
        _logger = logger;
        _context = context;
        _config = config.Value;
        _backupPaths = backupPaths;
        _appPaths = appPaths;
    }

    public async Task CreateDailyBackupAsync(CancellationToken ct = default)
    {
        if (!_config.AutoBackup)
        {
            _logger.LogInformation("AutoBackup deshabilitado.");
            return;
        }

        Directory.CreateDirectory(_backupPaths.BackupRoot);

        var dbPath = _appPaths.DbFile;
        var logsDir = _appPaths.LogsDir;

        if (string.IsNullOrWhiteSpace(dbPath) || !File.Exists(dbPath))
        {
            _logger.LogWarning("No se encontró la base de datos para backup. Ruta: {DbPath}", dbPath);
            return;
        }

        var fileName = $"gymadmin-daily-{DateTime.Now:yyyyMMdd}.zip";
        var zipPath = Path.Combine(_backupPaths.BackupRoot, fileName);

        if (File.Exists(zipPath))
        {
            _logger.LogInformation("Ya existe backup diario para hoy: {ZipPath}", zipPath);
            return;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"gymadmin-backup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            _logger.LogInformation("Iniciando backup diario. TempDir: {TempDir}", tempDir);

            // 1. Backup seguro de SQLite
            var fileDbName = Path.GetFileName(_appPaths.DbFile);
            var dbDest = Path.Combine(tempDir, fileDbName);
            _logger.LogInformation("Respaldando base SQLite desde {DbPath} hacia {DbDest}", dbPath, dbDest);
            await BackupDatabaseAsync(dbPath, dbDest, ct);

            // 2. Copia de logs
            if (Directory.Exists(logsDir))
            {
                var logsDest = Path.Combine(tempDir, "Logs");
                _logger.LogInformation("Copiando logs desde {LogsDir} hacia {LogsDest}", logsDir, logsDest);
                CopyLogsForBackup(logsDir, logsDest, ct);
            }
            else
            {
                _logger.LogInformation("La carpeta de logs no existe. Se omite. Ruta: {LogsDir}", logsDir);
            }

            // 3. Crear ZIP final
            _logger.LogInformation("Creando archivo ZIP en {ZipPath}", zipPath);
            ZipFile.CreateFromDirectory(tempDir, zipPath, CompressionLevel.Optimal, false);

            _logger.LogInformation("Backup diario creado correctamente: {ZipPath}", zipPath);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("La creación del backup diario fue cancelada.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando backup diario.");
            throw;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo limpiar la carpeta temporal del backup: {TempDir}", tempDir);
            }
        }
    }

    public Task CleanupOldBackupsAsync(CancellationToken ct = default)
    {
        Directory.CreateDirectory(_backupPaths.BackupRoot);

        var limitDate = DateTime.Now.Date.AddDays(-_config.RetentionDays);

        var files = Directory.EnumerateFiles(_backupPaths.BackupRoot, "*.zip", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            try
            {
                var creationDate = File.GetCreationTime(file);

                if (creationDate.Date < limitDate)
                {
                    File.Delete(file);
                    _logger.LogInformation("Backup eliminado por retención: {File}", file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error eliminando backup antiguo: {File}", file);
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> DailyBackupExistsForTodayAsync(CancellationToken ct = default)
    {
        var fileName = $"gymadmin-daily-{DateTime.Now:yyyyMMdd}.zip";
        var path = Path.Combine(_backupPaths.BackupRoot, fileName);

        return Task.FromResult(File.Exists(path));
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

    private void CopyLogsForBackup(string sourceDir, string destDir, CancellationToken ct)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            ct.ThrowIfCancellationRequested();

            var destFile = Path.Combine(destDir, Path.GetFileName(file));

            try
            {
                _logger.LogInformation("Copiando log al backup: {File}", file);
                CopyFileSnapshotWithRetry(file, destFile);
                _logger.LogInformation("Log copiado correctamente: {File}", file);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo copiar el log al backup: {File}", file);
            }
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            ct.ThrowIfCancellationRequested();

            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyLogsForBackup(dir, destSubDir, ct);
        }
    }

    private static void CopyFileSnapshotWithRetry(string source, string destination)
    {
        const int maxRetries = 3;
        const int delayMs = 200;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                CopyFileSnapshot(source, destination);
                return;
            }
            catch (IOException) when (attempt < maxRetries)
            {
                Thread.Sleep(delayMs);
            }
        }

        throw new IOException($"No se pudo copiar el archivo después de {maxRetries} intentos. Archivo: {source}");
    }

    private static void CopyFileSnapshot(string source, string destination)
    {
        const int bufferSize = 81920;

        using var sourceStream = new FileStream(
            source,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        using var destinationStream = new FileStream(
            destination,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        var lengthToCopy = sourceStream.Length;
        sourceStream.Position = 0;

        var buffer = new byte[bufferSize];
        long totalCopied = 0;

        while (totalCopied < lengthToCopy)
        {
            var remaining = lengthToCopy - totalCopied;
            var bytesToRead = (int)Math.Min(buffer.Length, remaining);

            var bytesRead = sourceStream.Read(buffer, 0, bytesToRead);
            if (bytesRead == 0)
                break;

            destinationStream.Write(buffer, 0, bytesRead);
            totalCopied += bytesRead;
        }

        destinationStream.Flush(true);
    }
}