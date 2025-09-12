using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GymAdmin.Desktop.ViewModels.Dialogs;

public partial class ConfirmDialogViewModel : ObservableObject
{
    private readonly TaskCompletionSource<bool> _tcs = new();

    [ObservableProperty] private string title = "Confirmar";
    [ObservableProperty] private string message = "¿Está seguro?";
    [ObservableProperty] private string acceptText = "Aceptar";
    [ObservableProperty] private string cancelText = "Cancelar";

    public Task<bool> Task => _tcs.Task;

    [RelayCommand] private void Accept() => _tcs.TrySetResult(true);
    [RelayCommand] private void Cancel() => _tcs.TrySetResult(false);
}
