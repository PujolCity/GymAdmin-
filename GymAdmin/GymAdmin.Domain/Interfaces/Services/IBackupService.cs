using GymAdmin.Domain.Results;

namespace GymAdmin.Domain.Interfaces.Services;

public interface IBackupService
{
    Task CreateDailyBackupAsync(CancellationToken ct = default);
    Task<Result<string>> CreateManualBackupAsync(CancellationToken ct = default);
    Task<bool> DailyBackupExistsForTodayAsync(CancellationToken ct = default);
    Task CleanupOldBackupsAsync(CancellationToken ct = default);
}
