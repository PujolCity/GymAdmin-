using GymAdmin.Infrastructure.Paths.BackupPaths;
using Microsoft.Data.Sqlite;

namespace GymAdmin.Infrastructure.Backup;

public class SqliteMigrationSafetyBackup : IMigrationSafetyBackup
{
    private readonly IBackupPaths _backupPaths;

    public SqliteMigrationSafetyBackup(IBackupPaths backupPaths)
    {
        _backupPaths = backupPaths;
    }

    public string CreatePreMigrationBackup(string dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
            throw new ArgumentException("La ruta de la base de datos es obligatoria.", nameof(dbPath));

        if (!File.Exists(dbPath))
            throw new FileNotFoundException("No se encontró el archivo de base de datos.", dbPath);

        var migrationBackupDir = Path.Combine(_backupPaths.BackupRoot, "MigrationBackups");
        Directory.CreateDirectory(migrationBackupDir);

        PruneOldMigrationBackups(migrationBackupDir);

        var backupPath = Path.Combine(
            migrationBackupDir,
            $"pre-migrate_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db");

        using var source = new SqliteConnection($"Data Source={dbPath};");
        using var destination = new SqliteConnection($"Data Source={backupPath};");

        source.Open();
        destination.Open();

        source.BackupDatabase(destination);

        return backupPath;
    }

    public void Restore(string backupPath, string dbPath)
    {
        if (string.IsNullOrWhiteSpace(backupPath))
            throw new ArgumentException("La ruta del backup es obligatoria.", nameof(backupPath));

        if (string.IsNullOrWhiteSpace(dbPath))
            throw new ArgumentException("La ruta de la base de datos es obligatoria.", nameof(dbPath));

        if (!File.Exists(backupPath))
            throw new FileNotFoundException("No se encontró el backup a restaurar.", backupPath);

        var dbDirectory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrWhiteSpace(dbDirectory))
            Directory.CreateDirectory(dbDirectory);

        File.Copy(backupPath, dbPath, overwrite: true);

        var walPath = dbPath + "-wal";
        var shmPath = dbPath + "-shm";

        if (File.Exists(walPath))
            File.Delete(walPath); 

        if (File.Exists(shmPath))
            File.Delete(shmPath);
    }

    private static void PruneOldMigrationBackups(string folder, int keepLast = 5)
    {
        var files = new DirectoryInfo(folder)
            .GetFiles("pre-migrate_*.db")
            .OrderByDescending(x => x.CreationTimeUtc)
            .ToList();

        foreach (var file in files.Skip(keepLast))
        {
            file.Delete();
        }
    }
}
