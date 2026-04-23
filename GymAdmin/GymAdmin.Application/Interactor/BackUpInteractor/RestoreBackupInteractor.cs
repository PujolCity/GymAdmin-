using FluentValidation;
using GymAdmin.Applications.DTOs.Asistencia;
using GymAdmin.Applications.DTOs.ConfiguracionDto;
using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;

namespace GymAdmin.Applications.Interactor.BackUpInteractor;

public class RestoreBackupInteractor : IRestoreBackupInteractor
{
    private readonly IBackupRestoreService _backupRestoreService;
    private readonly IValidator<RestoreBackupDto> _validator;

    public RestoreBackupInteractor(IBackupRestoreService backupRestoreService,
        IValidator<RestoreBackupDto> validator)
    {
        _backupRestoreService = backupRestoreService;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(RestoreBackupDto restoreBackupDto, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(restoreBackupDto, ct);
        if (!validation.IsValid)
            return Result.Fail(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            return await _backupRestoreService.RestoreDailyBackupAsync(restoreBackupDto.ZipFilePath, restoreBackupDto.RestoreLogs, ct);
        }
        catch (InvalidDataException)
        {
            return Result.Fail("El archivo ZIP está dañado o no tiene un formato válido.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error al restaurar backup: {ex.Message}");
        }
    }
}
