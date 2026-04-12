using GymAdmin.Domain.Results;

namespace GymAdmin.Domain.Interfaces.Services;

public interface IBackupRestoreService
{
    Task<Result> RestoreDailyBackupAsync(string zipFilePath, bool restoreLogs, CancellationToken ct = default);
}