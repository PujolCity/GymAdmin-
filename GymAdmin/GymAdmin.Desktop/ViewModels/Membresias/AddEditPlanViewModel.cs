using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Applications.Interactor.PlanesMembresia;
using GymAdmin.Domain.Results;
using System.ComponentModel;
using System.Globalization;

namespace GymAdmin.Desktop.ViewModels.Membresias;

public partial class AddEditPlanViewModel : ViewModelBase, IDataErrorInfo, IDisposable
{
    private readonly ICreateOrUpdatePlanInteractor _upsert;
    private CancellationTokenSource? _cts;

    public AddEditPlanViewModel(ICreateOrUpdatePlanInteractor upsert)
    {
        _upsert = upsert;
        Activo = true;
        Titulo = "Nuevo Plan de Membresía";

        // Si el CanExecute de Guardar depende de IsBusy, enlazalo:
        BindBusyToCommands(GuardarCommand);
    }

    // -------------------- Estado --------------------
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string titulo = "Plan";

    // -------------------- Campos --------------------
    [ObservableProperty] private int id;
    [ObservableProperty] private string nombre = string.Empty;
    [ObservableProperty] private string descripcion = string.Empty;

    // strings para validar
    [ObservableProperty] private string creditos = string.Empty;
    [ObservableProperty] private string duracionDias = string.Empty;
    [ObservableProperty] private string precioSugerido = string.Empty;

    [ObservableProperty] private bool activo = true;

    // -------------------- Validación (IDataErrorInfo) --------------------
    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(Nombre) => string.IsNullOrWhiteSpace(Nombre) ? "El nombre es obligatorio." : string.Empty,
        nameof(Creditos) => !int.TryParse(Creditos, out var c) || c <= 0 ? "Créditos debe ser un entero > 0." : string.Empty,
        nameof(DuracionDias) => !int.TryParse(DuracionDias, out var d) || d <= 0 ? "Duración debe ser un entero > 0." : string.Empty,
        nameof(PrecioSugerido) => !decimal.TryParse(PrecioSugerido, NumberStyles.Number, CultureInfo.InvariantCulture, out var p) || p < 0 ? "Precio debe ser un número ≥ 0." : string.Empty,
        _ => string.Empty
    };

    public bool FormularioValido =>
        string.IsNullOrEmpty(this[nameof(Nombre)]) &&
        string.IsNullOrEmpty(this[nameof(Creditos)]) &&
        string.IsNullOrEmpty(this[nameof(DuracionDias)]) &&
        string.IsNullOrEmpty(this[nameof(PrecioSugerido)]);

    partial void OnNombreChanged(string value) => NotifyValidationChanged();
    partial void OnCreditosChanged(string value) => NotifyValidationChanged();
    partial void OnDuracionDiasChanged(string value) => NotifyValidationChanged();
    partial void OnPrecioSugeridoChanged(string value) => NotifyValidationChanged();

    private void NotifyValidationChanged()
    {
        OnPropertyChanged(nameof(FormularioValido));
        GuardarCommand.NotifyCanExecuteChanged();
    }

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

    // -------------------- Commands --------------------
    [RelayCommand(CanExecute = nameof(CanGuardar))]
    private async Task GuardarAsync()
    {
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
                Nombre = Nombre.Trim(),
                Descripcion = Descripcion?.Trim(),
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

    [RelayCommand]
    private void Cancelar() => RequestClose();

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}