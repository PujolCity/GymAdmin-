using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.ConfiguracionDto;
using GymAdmin.Applications.Interactor.BackUpInteractor;
using GymAdmin.Desktop.Services;
using GymAdmin.Domain.Results;
using GymAdmin.Infrastructure.Paths.BackupPaths;
using System.Diagnostics;
using System.IO;

namespace GymAdmin.Desktop.ViewModels.Configuracion;

public partial class BackupViewModel : ViewModelBase, IDisposable, IDialogHost
{
    private readonly IFileDialogService _fileDialogService;
    private readonly IRestoreBackupInteractor _restoreBackupInteractor;
    private readonly IBackupPaths _backupPaths;
    private readonly IOverlayDialogService _dialogService;
    private readonly ICreateManualBackup _createManualBackup;

    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _infoMessageCts;

    private const int INFO_MESSAGE_DURATION_MS = 5000;
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
        IOverlayDialogService dialogService,
        ICreateManualBackup createManualBackup)
    {
        _fileDialogService = fileDialogService;
        _restoreBackupInteractor = restoreBackupInteractor;
        _backupPaths = backupPaths;
        BindBusyToCommands(SeleccionarBackupCommand, RestaurarBackupCommand, AbrirCarpetaBackupsCommand, CrearBackupManualCommand);
        _dialogService = dialogService;
        _createManualBackup = createManualBackup;
    }

    private bool CanRestore()
        => !IsBusy && !string.IsNullOrWhiteSpace(SelectedBackupPath);
    private bool CanSimpleAction() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void SeleccionarBackup()
    {
        try
        {
            ClearMessages();

            var initialDirectory = Directory.Exists(_backupPaths.BackupRoot)
                ? _backupPaths.BackupRoot
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var path = _fileDialogService.SelectZipFile(initialDirectory);

            if (string.IsNullOrWhiteSpace(path))
                return;

            SelectedBackupPath = path;
            SetInfo("Backup seleccionado correctamente.");
        }
        catch (Exception ex)
        {
            SetError($"Error al seleccionar backup: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanRestore))]
    private async Task RestaurarBackup(CancellationToken ct = default)
    {
        try
        {
            ClearMessages();

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

                var dto = new RestoreBackupDto
                {
                    ZipFilePath = SelectedBackupPath!,
                    RestoreLogs = confirmLogs,
                };

                result = await _restoreBackupInteractor.ExecuteAsync(dto, ct);
            }

            if (!result.IsSuccess)
            {
                SetError(string.Join(Environment.NewLine, result.Errors));
                return;
            }

            SetInfo("Backup restaurado correctamente.");

            await _dialogService.ShowConfirmAsync(
                this,
                InfoMessage!,
                "La base de datos fue restaurada correctamente.\n\n" +
                "Se recomienda reiniciar la aplicación.",
                accept: "Aceptar",
                string.Empty,
                COLOR_PRIMARIO, COLOR_ROJO, false);
        }
        catch (Exception ex)
        {
            SetError($"Error al restaurar backup: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSimpleAction))]
    private void AbrirCarpetaBackups()
    {
        try
        {
            ClearMessages();

            var backupRoot = _backupPaths.BackupRoot;

            if (string.IsNullOrWhiteSpace(backupRoot))
            {
                SetError("No hay una carpeta de backups configurada.");
                return;
            }

            if (!Directory.Exists(backupRoot))
            {
                Directory.CreateDirectory(backupRoot);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = backupRoot,
                UseShellExecute = true
            });

            SetInfo("Carpeta de backups abierta correctamente.");
        }
        catch (Exception ex)
        {
            SetError($"Error al abrir la carpeta de backups: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CrearBackupManual(CancellationToken ct = default)
    {
        if (IsBusy) return;

        try
        {
            ClearMessages();
            IsBusy = true;

            var result = await _createManualBackup.ExecuteAsync(ct);

            if (!result.IsSuccess)
            {
                SetError(string.Join(Environment.NewLine, result.Errors));
                return;
            }

            SetInfo("Backup creado correctamente.");

            await _dialogService.ShowConfirmAsync(
                this,
                InfoMessage!,
                $"Backup creado: {result.Value}",
                accept: "Aceptar",
                string.Empty,
                COLOR_PRIMARIO,
                COLOR_ROJO,
                false);
        }
        catch (Exception ex)
        {
            SetError($"Error al crear backup manual: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearError() => ErrorMessage = null;

    [RelayCommand]
    private void ClearInfo() => InfoMessage = null;

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private void ClearMessages()
    {
        ErrorMessage = null;
        InfoMessage = null;
    }

    private void SetError(string message)
    {
        _infoMessageCts?.Cancel();

        ErrorMessage = message;
        InfoMessage = null;
    }

    private async void SetInfo(string message)
    {
        ErrorMessage = null;
        InfoMessage = message;

        _infoMessageCts?.Cancel();
        _infoMessageCts = new CancellationTokenSource();
        var token = _infoMessageCts.Token;

        try
        {
            await Task.Delay(INFO_MESSAGE_DURATION_MS, token);

            if (!token.IsCancellationRequested)
            {
                InfoMessage = null;
            }
        }
        catch (TaskCanceledException)
        {
        }
    }
}
