
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.SociosDto;
using GymAdmin.Applications.Interactor.SociosInteractors;
using GymAdmin.Applications.Interfaces.ValidacionesUI;
using System.ComponentModel;
using System.Windows;

namespace GymAdmin.Desktop.ViewModels.Socios;

public sealed partial class AddSocioViewModel : ObservableObject, IDataErrorInfo
{
    private readonly IValidationUIService _validationUIService;
    private readonly ISocioCreateInteractor _socioCreateInteractor;

    public event Action? CloseRequested;
    private void RequestClose() => CloseRequested?.Invoke();

    [ObservableProperty]
    private bool hasUserInteracted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormularioValido))]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    private string dni = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormularioValido))]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    private string nombre = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormularioValido))]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    private string apellido = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    [NotifyPropertyChangedFor(nameof(FormularioValido))]
    private string telefono = string.Empty;

    // Marcamos interacción al escribir
    partial void OnDniChanged(string value) => MarkAsInteracted();
    partial void OnNombreChanged(string value) => MarkAsInteracted();
    partial void OnApellidoChanged(string value) => MarkAsInteracted();
    partial void OnTelefonoChanged(string value) => MarkAsInteracted();

    public bool FormularioValido => string.IsNullOrEmpty(Error);

    public AddSocioViewModel(
        ISocioCreateInteractor socioCreateInteractor,
        IValidationUIService validationUIService)
    {
        _socioCreateInteractor = socioCreateInteractor;
        _validationUIService = validationUIService;
    }

    private void MarkAsInteracted()
    {
        if (!HasUserInteracted)
        {
            HasUserInteracted = true;

            OnPropertyChanged(nameof(Dni));
            OnPropertyChanged(nameof(Nombre));
            OnPropertyChanged(nameof(Apellido));
            OnPropertyChanged(nameof(Telefono));
            OnPropertyChanged(nameof(FormularioValido));
        }
    }

    public string Error
    {
        get
        {
            var e1 = this[nameof(Dni)];
            if (!string.IsNullOrEmpty(e1)) return e1;

            var e2 = this[nameof(Nombre)];
            if (!string.IsNullOrEmpty(e2)) return e2;

            var e3 = this[nameof(Apellido)];
            if (!string.IsNullOrEmpty(e3)) return e3;

            var e4 = this[nameof(Telefono)];
            if (!string.IsNullOrEmpty(e4)) return e4;

            return string.Empty;
        }
    }

    public string this[string columnName]
    {
        get
        {
            if (!HasUserInteracted) return string.Empty;

            return columnName switch
            {
                nameof(Dni) => ValidarDni(Dni),
                nameof(Nombre) => ValidarNombre(Nombre),
                nameof(Apellido) => ValidarApellido(Apellido),
                nameof(Telefono) => ValidarTelefono(Telefono),
                _ => string.Empty
            };
        }
    }

    private string ValidarDni(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "DNI es requerido";

        if (!_validationUIService.EsDniValido(value))
            return "DNI debe tener exactamente 8 dígitos";

        return string.Empty;
    }

    private string ValidarNombre(string value)
    {
        var errores = _validationUIService.ValidarNombreCompleto(value, "nombre");
        return errores.Count > 0 ? errores[0] : string.Empty;
    }

    private string ValidarApellido(string value)
    {
        var errores = _validationUIService.ValidarNombreCompleto(value, "apellido");
        return errores.Count > 0 ? errores[0] : string.Empty;
    }

    private string ValidarTelefono(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty; 
        if (!_validationUIService.EsTelefonoValido(value))
            return "Teléfono debe tener entre 7 y 15 dígitos";
        return string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanGuardar))]
    private async Task GuardarAsync()
    {
        try
        {
            HasUserInteracted = true;
            OnPropertyChanged(nameof(Dni));
            OnPropertyChanged(nameof(Nombre));
            OnPropertyChanged(nameof(Apellido));
            OnPropertyChanged(nameof(FormularioValido));

            if (!FormularioValido)
            {
                MessageBox.Show("Por favor corrija los errores en el formulario", "Error de validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new SocioCreateDto
            {
                Dni = Dni?.Trim(),
                Nombre = Nombre?.Trim(),
                Apellido = Apellido?.Trim(),
                Telefono = Telefono?.Trim()
            };

            var result = await _socioCreateInteractor.ExecuteAsync(dto);
            if (result.IsSuccess)
            {
                MessageBox.Show("Socio creado exitosamente", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose();
            }
            else
            {
                MessageBox.Show(string.Join(Environment.NewLine, result.Errors), "Error al guardar",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error inesperado: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool CanGuardar() => FormularioValido;

    [RelayCommand]
    private void Cancelar() => RequestClose();
}