namespace GymAdmin.Infrastructure.Backup;

public interface IMigrationSafetyBackup
{
    string CreatePreMigrationBackup(string dbPath);
    void Restore(string backupPath, string dbPath);
}
