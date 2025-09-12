using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GymAdmin.Desktop.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    public event Action? CloseRequested;
    protected void RequestClose() => CloseRequested?.Invoke();

    // Notificación de cambio de IsBusy para que los hijos puedan engancharse
    public event Action<bool>? BusyChanged;

    partial void OnIsBusyChanged(bool value)
    {
        // dispara el hook virtual (si algún hijo lo overridea)
        OnIsBusyChangedCore(value);

        // notifica a todos los subscriptores (los que hicieron BindBusyToCommands)
        BusyChanged?.Invoke(value);
    }

    protected virtual void OnIsBusyChangedCore(bool newValue) { }

    /// <summary>
    /// Ata IsBusy a uno o más IRelayCommand para disparar NotifyCanExecuteChanged automáticamente.
    /// Llamalo en el constructor del VM hijo (después de inicializar los comandos).
    /// </summary>
    protected void BindBusyToCommands(params IRelayCommand[] commands)
    {
        BusyChanged += _ =>
        {
            foreach (var cmd in commands)
                cmd.NotifyCanExecuteChanged();
        };
    }
}
