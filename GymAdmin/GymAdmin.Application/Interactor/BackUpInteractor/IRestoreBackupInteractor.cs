using GymAdmin.Applications.DTOs.ConfiguracionDto;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.BackUpInteractor;

public  interface IRestoreBackupInteractor
{
    Task<Result> ExecuteAsync(RestoreBackupDto restoreBackupDto, CancellationToken ct = default);
}
