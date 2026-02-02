namespace GymAdmin.Desktop.ViewModels.Configuracion;

public partial class ConfigGeneralViewModel : ViewModelBase, IDisposable
{
    private CancellationTokenSource? _cts;

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
