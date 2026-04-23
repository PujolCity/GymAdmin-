using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.BackUpInteractor;

public interface ICreateManualBackup
{
    Task<Result<string>> ExecuteAsync(CancellationToken ct);
}
