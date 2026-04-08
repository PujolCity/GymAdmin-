namespace GymAdmin.Infrastructure.Backup.DailyBackup;

public interface IBackupService
{
    Task CreateDailyBackupAsync(CancellationToken ct = default);
    Task<bool> DailyBackupExistsForTodayAsync(CancellationToken ct = default);
    Task CleanupOldBackupsAsync(CancellationToken ct = default);
}
