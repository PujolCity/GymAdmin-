using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.Interactor.BackUpInteractor;
using GymAdmin.Desktop.Services;
using GymAdmin.Domain.Results;
using GymAdmin.Infrastructure.Paths.BackupPaths;
using System.IO;

namespace GymAdmin.Desktop.ViewModels.Configuracion;

public partial class BackupViewModel : ViewModelBase, IDisposable, IDialogHost
{
    private readonly IFileDialogService _fileDialogService;
    private readonly IRestoreBackupInteractor _restoreBackupInteractor;
    private readonly IBackupPaths _backupPaths;
    private readonly IOverlayDialogService _dialogService;

    private CancellationTokenSource? _cts;


    private const string COLOR_ROJO = "DangerButtonStyle";
    private const string COLOR_PRIMARIO = "PrimaryButtonStyle";
    private const string COLOR_AMARILLO = "WarningButtonStyle";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestaurarBackupCommand))]
    private string? selectedBackupPath;

    [ObservableProperty]
    private bool restoreLogs = true;

    [ObservableProperty]
    private string? infoMessage;

    [ObservableProperty]
    private object? dialogContent;

    [ObservableProperty]
    private bool isDialogOpen;

    public BackupViewModel(IFileDialogService fileDialogService,
        IRestoreBackupInteractor restoreBackupInteractor,
        IBackupPaths backupPaths,
        IOverlayDialogService dialogService)
    {
        _fileDialogService = fileDialogService;
        _restoreBackupInteractor = restoreBackupInteractor;
        _backupPaths = backupPaths;
        BindBusyToCommands(SeleccionarBackupCommand, RestaurarBackupCommand);
        _dialogService = dialogService;
    }

    private bool CanRestore()
        => !IsBusy && !string.IsNullOrWhiteSpace(SelectedBackupPath);
    private bool CanSimpleAction() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void SeleccionarBackup()
    {
        try
        {
            ErrorMessage = null;
            InfoMessage = null;

            var initialDirectory = Directory.Exists(_backupPaths.BackupRoot)
                ? _backupPaths.BackupRoot
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var path = _fileDialogService.SelectZipFile(initialDirectory);

            if (string.IsNullOrWhiteSpace(path))
                return;

            SelectedBackupPath = path;
            InfoMessage = "Backup seleccionado correctamente.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al seleccionar backup: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanRestore))]
    private async Task RestaurarBackup(CancellationToken ct = default)
    {
        try
        {
            ErrorMessage = null;
            InfoMessage = null;
            IsBusy = true;

            var confirmRestauracion = await _dialogService.ShowConfirmAsync(
               this,
                "Restaurar backup",
                "Se va a restaurar la base de datos desde el archivo seleccionado.\n\n" +
                "La base de datos actual será reemplazada.\n" +
                "Se generará un backup de seguridad antes de continuar.\n\n" +
                "¿Desea continuar? \n\n La aplicación podría necesitar reiniciarse luego del proceso.",
               accept: "Restaurar",
               cancel: "Cancelar",
               COLOR_AMARILLO,
               COLOR_ROJO);

            Result result = new();
            if (confirmRestauracion)
            {
                var confirmLogs = await _dialogService.ShowConfirmAsync(
                this,
                "Restaurar logs",
                "El backup contiene archivos de logs.\n\n" +
                "¿Desea restaurarlos también?",
                accept: "Aceptar",
                cancel: "No",
                COLOR_PRIMARIO, COLOR_ROJO);

                result = await _restoreBackupInteractor.ExecuteAsync(SelectedBackupPath!, confirmLogs, ct);
            }

            if (!result.IsSuccess)
            {
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
                return;
            }

            InfoMessage = "Backup restaurado correctamente.";
            await _dialogService.ShowConfirmAsync(
                this,
                InfoMessage,
                "La base de datos fue restaurada correctamente.\n\n" +
                "Se recomienda reiniciar la aplicación.",
                accept: "Aceptar",
                string.Empty,
                COLOR_PRIMARIO, COLOR_ROJO, false);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al restaurar backup: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearError() => ErrorMessage = null;

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }


}
