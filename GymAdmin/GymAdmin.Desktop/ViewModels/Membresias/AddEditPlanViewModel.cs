using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Applications.Interactor.PlanesMembresia;
using GymAdmin.Applications.Interfaces.ValidacionesUI;
using GymAdmin.Domain.Results;
using System.ComponentModel;
using System.Globalization;

namespace GymAdmin.Desktop.ViewModels.Membresias;

public partial class AddEditPlanViewModel : ViewModelBase, IDataErrorInfo, IDisposable
{
    private readonly ICreateOrUpdatePlanInteractor _upsert;
    private readonly IValidationUIService _validationUI;
    private CancellationTokenSource? _cts;

    public AddEditPlanViewModel(ICreateOrUpdatePlanInteractor upsert, IValidationUIService validationUI)
    {
        _upsert = upsert;
        _validationUI = validationUI;

        Activo = true;
        Titulo = "Nuevo Plan de Membresía";

        BindBusyToCommands(GuardarCommand);
    }

    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string titulo = "Plan";

    [ObservableProperty] private int id;
    [ObservableProperty] private string nombre = string.Empty;
    [ObservableProperty] private string descripcion = string.Empty;
    [ObservableProperty] private string creditos = string.Empty;      // numérico (string para validar)
    [ObservableProperty] private string duracionDias = string.Empty;  // numérico
    [ObservableProperty] private string precioSugerido = string.Empty;// numérico
    [ObservableProperty] private bool activo = true;

    [ObservableProperty] private bool hasNombreInteracted;
    [ObservableProperty] private bool hasCreditosInteracted;
    [ObservableProperty] private bool hasDuracionInteracted;
    [ObservableProperty] private bool hasPrecioInteracted;

    partial void OnNombreChanged(string value) { HasNombreInteracted = true; NotifyValidationChanged(); }
    partial void OnCreditosChanged(string value) { HasCreditosInteracted = true; NotifyValidationChanged(); }
    partial void OnDuracionDiasChanged(string value) { HasDuracionInteracted = true; NotifyValidationChanged(); }
    partial void OnPrecioSugeridoChanged(string value) { HasPrecioInteracted = true; NotifyValidationChanged(); }

    private void NotifyValidationChanged()
    {
        OnPropertyChanged(nameof(FormularioValido));
        GuardarCommand.NotifyCanExecuteChanged();
    }

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(Nombre) =>
            !HasNombreInteracted ? string.Empty :
            ValidarNombre(Nombre),

        nameof(Creditos) =>
            !HasCreditosInteracted ? string.Empty :
            (!int.TryParse(Creditos, NumberStyles.Integer, CultureInfo.InvariantCulture, out var c) || c <= 0
                ? "Créditos debe ser un entero > 0."
                : string.Empty),

        nameof(DuracionDias) =>
            !HasDuracionInteracted ? string.Empty :
            (!int.TryParse(DuracionDias, NumberStyles.Integer, CultureInfo.InvariantCulture, out var d) || d <= 0
                ? "Duración debe ser un entero > 0."
                : string.Empty),

        nameof(PrecioSugerido) =>
            !HasPrecioInteracted ? string.Empty :
            (!decimal.TryParse(PrecioSugerido, NumberStyles.Number, CultureInfo.InvariantCulture, out var p) || p < 0
                ? "Precio debe ser un número ≥ 0."
                : string.Empty),

        _ => string.Empty
    };

    private string ValidarNombre(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "El nombre es obligatorio.";

        var errores = _validationUI.ValidarNombreCompleto(value, "nombre");
        return errores.Count > 0 ? errores[0] : string.Empty;
    }

    public bool FormularioValido =>
        string.IsNullOrEmpty(ForceValidate(nameof(Nombre))) &&
        string.IsNullOrEmpty(ForceValidate(nameof(Creditos))) &&
        string.IsNullOrEmpty(ForceValidate(nameof(DuracionDias))) &&
        string.IsNullOrEmpty(ForceValidate(nameof(PrecioSugerido)));

    // Evalúa usando el indexer, respetando flags actuales
    private string ForceValidate(string propertyName) => this[propertyName];

    // Para edición:
    public void CargarParaEdicion(PlanMembresiaDto plan)
    {
        Id = plan.Id;
        Nombre = plan.Nombre ?? string.Empty;
        Descripcion = plan.Descripcion ?? string.Empty;
        Creditos = plan.Creditos.ToString(CultureInfo.InvariantCulture);
        DuracionDias = plan.DiasValidez.ToString(CultureInfo.InvariantCulture);
        PrecioSugerido = plan.Precio.ToString(CultureInfo.InvariantCulture);
        Activo = plan.IsActive;

        Titulo = "Editar Plan de Membresía";
    }

    [RelayCommand(CanExecute = nameof(CanGuardar))]
    private async Task GuardarAsync()
    {
        // Forzar “mostrar errores” si el usuario no tocó algún campo (p.ej. click directo en Guardar)
        if (!HasNombreInteracted) { HasNombreInteracted = true; OnPropertyChanged(nameof(Nombre)); }
        if (!HasCreditosInteracted) { HasCreditosInteracted = true; OnPropertyChanged(nameof(Creditos)); }
        if (!HasDuracionInteracted) { HasDuracionInteracted = true; OnPropertyChanged(nameof(DuracionDias)); }
        if (!HasPrecioInteracted) { HasPrecioInteracted = true; OnPropertyChanged(nameof(PrecioSugerido)); }
        OnPropertyChanged(nameof(FormularioValido));

        if (!FormularioValido) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var dto = new PlanMembresiaDto
            {
                Id = Id,
                Nombre = _validationUI.LimpiarYFormatearTexto(Nombre),
                Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : _validationUI.LimpiarYFormatearTexto(Descripcion),
                Creditos = int.Parse(Creditos, CultureInfo.InvariantCulture),
                DiasValidez = int.Parse(DuracionDias, CultureInfo.InvariantCulture),
                Precio = decimal.Parse(PrecioSugerido, CultureInfo.InvariantCulture),
                IsActive = Activo
            };

            Result result = await _upsert.ExecuteAsync(dto, _cts.Token);
            if (result.IsSuccess)
            {
                RequestClose();
            }
            else
            {
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGuardar() => FormularioValido && !IsBusy;

    [RelayCommand] private void Cancelar() => RequestClose();

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}