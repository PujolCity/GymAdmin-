namespace GymAdmin.Desktop.ViewModels.Configuracion;

public partial class ConfigViewModel : ViewModelBase, IDisposable
{
    private CancellationTokenSource? _cts;
    public MetodoPagoViewModel MetodoPagoViewModel { get; }
    public ConfigGeneralViewModel ConfigGeneralViewModel { get; }

    public ConfigViewModel(MetodoPagoViewModel metodoPagoViewModel,
        ConfigGeneralViewModel configGeneralViewModel)
    {
        MetodoPagoViewModel = metodoPagoViewModel;
        ConfigGeneralViewModel = configGeneralViewModel;
    }

    public ConfigViewModel()
    {
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    //[RelayCommand]
    //private void BrowseDefaultFolder()
    //{
    //    using var dlg = new FolderBrowserDialog
    //    {
    //        Description = "Seleccioná la carpeta base para exportaciones/plantillas"
    //    };

    //    if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
    //        DefaultFolder = dlg.SelectedPath;
    //}

    //[RelayCommand]
    //private void SaveGeneral()
    //{
    //    var error = Validate();
    //    if (error is not null)
    //    {
    //        System.Windows.MessageBox.Show(error, "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
    //        return;
    //    }

    //    if (!Directory.Exists(DefaultFolder))
    //    {
    //        try { Directory.CreateDirectory(DefaultFolder); }
    //        catch (IOException ex)
    //        {
    //            System.Windows.MessageBox.Show($"No se pudo crear la carpeta base:\n{ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    //            return;
    //        }
    //    }

    //    //_settings.SaveGeneral(new GeneralSettingsDto
    //    //{
    //    //    NombreGimnasio = TradeName,
    //    //    Telefono = PhoneOrWhatsapp,   // el servicio se encarga de encriptar
    //    ////    EmailContacto = Email,
    //    //    CarpetaBase = DefaultFolder
    //    //});

    //    System.Windows.MessageBox.Show("Configuración guardada.", "Configuración", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    //}

    //private string Validate()
    //{
    //    //if (string.IsNullOrWhiteSpace(TradeName)) return "El nombre del gimnasio es obligatorio.";
    //    //if (string.IsNullOrWhiteSpace(PhoneOrWhatsapp)) return "El teléfono o WhatsApp es obligatorio.";
    //    if (string.IsNullOrWhiteSpace(Email)) return "El email es obligatorio.";
    //    if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return "El formato de email no es válido.";
    //    if (string.IsNullOrWhiteSpace(DefaultFolder)) return "La carpeta base es obligatoria.";
    //    return null;
    //}
}
