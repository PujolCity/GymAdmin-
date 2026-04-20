using GymAdmin.Domain.Interfaces.Services;
using GymAdmin.Domain.Results;
using System.IO.Compression;

namespace GymAdmin.Applications.Interactor.BackUpInteractor;

public class RestoreBackupInteractor : IRestoreBackupInteractor
{
    private readonly IBackupRestoreService _backupRestoreService;

    public RestoreBackupInteractor(IBackupRestoreService backupRestoreService)
    {
        _backupRestoreService = backupRestoreService;
    }

    public async Task<Result> ExecuteAsync(string zipFilePath, bool restoreLogs, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(zipFilePath))
            return Result.Fail("Debe seleccionar un archivo ZIP.");

        if (!File.Exists(zipFilePath))
            return Result.Fail("El archivo seleccionado no existe.");

        if (!string.Equals(Path.GetExtension(zipFilePath), ".zip", StringComparison.OrdinalIgnoreCase))
            return Result.Fail("El archivo seleccionado no es un archivo ZIP válido.");

        try
        {
            var result = await _backupRestoreService.RestoreDailyBackupAsync(zipFilePath, restoreLogs, ct);

            return result;
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
