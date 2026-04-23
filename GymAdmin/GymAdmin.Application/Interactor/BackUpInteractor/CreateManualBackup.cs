using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.BackUpInteractor;

public class CreateManualBackup : ICreateManualBackup
{
    private readonly IBackupService _backupService;

    public CreateManualBackup(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public async Task<Result<string>> ExecuteAsync(CancellationToken ct)
    {
        var result = await _backupService.CreateManualBackupAsync(ct);

        return result;
    }
}
