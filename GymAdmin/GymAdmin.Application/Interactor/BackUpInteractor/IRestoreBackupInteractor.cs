using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.BackUpInteractor;

public  interface IRestoreBackupInteractor
{
    Task<Result> ExecuteAsync(string zipFilePath, bool restoreLogs, CancellationToken ct = default);
}
