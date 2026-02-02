using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.MembresiasDto;
using GymAdmin.Applications.DTOs.MetodosPagoDto;
using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Applications.Interactor.ConfiguracionInteractors.MetodoPago;
using GymAdmin.Applications.Interactor.PagosInteractors;
using GymAdmin.Applications.Interactor.PlanesMembresia;
using GymAdmin.Applications.Interfaces.ValidacionesUI;
using GymAdmin.Domain.Enums;
using GymAdmin.Domain.Factory.CalculadorAjusteFactory;
using GymAdmin.Domain.Interfaces.Bussiness;
using GymAdmin.Domain.Results;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace GymAdmin.Desktop.ViewModels.Pagos;

public sealed partial class AddPagoViewModel : ViewModelBase, IDataErrorInfo, IDisposable
{
    private readonly IGetPlanesMembresiaInteractor _getPlanes;
    private readonly IGetMetodosPagoInteractor _getMetodosPago;
    private readonly ICreatePagoInteractor _createPago;
    private readonly IValidationUIService _validation;

    private readonly IGetSociosLookupInteractor? _sociosLookup;

    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _searchCts;

    public ObservableCollection<PlanMembresiaDto> Planes { get; } = [];
    public ObservableCollection<MetodoPagoDto> MetodosPago { get; } = [];

    [ObservableProperty] private SocioLookupDto? socioSeleccionado;

    [ObservableProperty] private bool showSocioPicker = true;

    public string SocioResumen => SocioSeleccionado is null
        ? "—"
        : $"{SocioSeleccionado.NombreCompleto} (DNI {SocioSeleccionado.Dni})";

    [ObservableProperty] private bool hasSocioInteracted;

    partial void OnSocioSeleccionadoChanged(SocioLookupDto? value)
    {
        HasSocioInteracted = true;
        OnPropertyChanged(nameof(SocioResumen));
        OnPropertyChanged(nameof(FormularioValido));
        GuardarCommand.NotifyCanExecuteChanged();
    }

    public ObservableCollection<SocioLookupDto> SociosSugeridos { get; } = new();

    [ObservableProperty] private string socioBusqueda = string.Empty;

    partial void OnSocioBusquedaChanged(string value)
    {
        HasSocioInteracted = true;
        _ = DebouncedBuscarSociosAsync();
    }

    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string titulo = "Registrar Pago";

    [ObservableProperty] private bool hasPlanInteracted;
    [ObservableProperty] private bool hasMetodoInteracted;
    [ObservableProperty] private bool hasPrecioInteracted;
    [ObservableProperty] private bool hasFechaPagoInteracted;
    [ObservableProperty] private bool hasVencimientoInteracted;
    
    [ObservableProperty] private bool precioEditadoManualmente;
    partial void OnPrecioChanged(string value)
    {
        HasPrecioInteracted = true;

        if (!_isSettingPrecioFromPlan)
            PrecioEditadoManualmente = true;

        RecalcularTotales();
        GuardarCommand.NotifyCanExecuteChanged();
    }

    private bool _isSettingVencimientoAuto;
    private bool _isSettingPrecioFromPlan;

    private void SetPrecioFromPlan(decimal precio)
    {
        _isSettingPrecioFromPlan = true;
        try
        {
            PrecioEditadoManualmente = false; // el plan manda
            Precio = precio.ToString(CultureInfo.InvariantCulture);
        }
        finally
        {
            _isSettingPrecioFromPlan = false;
        }
    }

    private void SetVencimientoAuto(DateTime fechaPagoBase, int diasValidez)
    {
        _isSettingVencimientoAuto = true;
        try
        {
            HasVencimientoInteracted = false; // porque lo setea el sistema
            FechaVencimiento = fechaPagoBase.Date.AddDays(diasValidez);
        }
        finally
        {
            _isSettingVencimientoAuto = false;
        }
    }


    [ObservableProperty] private PlanMembresiaDto? planSeleccionado;
    partial void OnPlanSeleccionadoChanged(PlanMembresiaDto? value)
    {
        HasPlanInteracted = true;

        if (value is not null && !PrecioEditadoManualmente)
            SetPrecioFromPlan(value.Precio);

        if (value is not null && value.DiasValidez > 0 && !HasVencimientoInteracted)
            SetVencimientoAuto(FechaPago ?? DateTime.Today, value.DiasValidez);

        if (value is not null)
            CreditosAsignados = value.Creditos;

        RecalcularTotales();
        GuardarCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty] private MetodoPagoDto? metodoSeleccionado;
    partial void OnMetodoSeleccionadoChanged(MetodoPagoDto? value)
    {
        HasMetodoInteracted = true;
        RecalcularTotales();
        GuardarCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty] private string precio = string.Empty;
  
    private void RecalcularTotales()
    {
        OnPropertyChanged(nameof(FormularioValido));
        OnPropertyChanged(nameof(MontoFinal));
        OnPropertyChanged(nameof(MontoFinalDisplay));
        OnPropertyChanged(nameof(Delta));
    }

    [ObservableProperty] private DateTime? fechaPago = DateTime.Now;
    partial void OnFechaPagoChanged(DateTime? value)
    {
        HasFechaPagoInteracted = true;
        if (PlanSeleccionado is not null && PlanSeleccionado.DiasValidez > 0)
            FechaVencimiento = (value ?? DateTime.Today).Date.AddDays(PlanSeleccionado.DiasValidez);

        OnPropertyChanged(nameof(FormularioValido));
        GuardarCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty] private int creditosAsignados;
    [ObservableProperty] private DateTime? fechaVencimiento;
    partial void OnFechaVencimientoChanged(DateTime? value)
    {
        if (!_isSettingVencimientoAuto)
            HasVencimientoInteracted = true;

        OnPropertyChanged(nameof(FormularioValido));
        GuardarCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty] private string observaciones = string.Empty;

    public decimal? MontoFinal => CalcularMontoFinal();
    public string MontoFinalDisplay =>
        MontoFinal is null ? "—" : MontoFinal.Value.ToString("C", CultureInfo.CurrentCulture);
    
    public decimal? Delta => CalcularDelta();

    private decimal? CalcularDelta()
    {
        if (!TryGetCalculoContext(out var basePrice, out var calculador, out var valorAjuste))
            return null;

        return calculador.CalcularDelta(basePrice, valorAjuste);
    }

    private decimal? CalcularMontoFinal()
    {
        if (!TryGetCalculoContext(out var basePrice, out var calculador, out var valorAjuste))
            return null;

        var montoFinal = calculador.Calcular(basePrice, valorAjuste);
        return Math.Max(0m, montoFinal);
    }

    private bool TryGetCalculoContext(out decimal basePrice,
    out ICalculadorTipoAjuste calculador, out decimal valorAjuste)
    {
        basePrice = 0;
        calculador = default!;
        valorAjuste = 0;

        if (MetodoSeleccionado is null)
            return false;

        if (!decimal.TryParse(Precio, NumberStyles.Number, CultureInfo.InvariantCulture, out basePrice))
            return false;

        if (basePrice < 0)
            return false;

        calculador = CalculadorAjusteFactory.Crear(MetodoSeleccionado.TipoAjuste);
        valorAjuste = MetodoSeleccionado.ValorAjuste;

        return true;
    }

    public AddPagoViewModel(
        IGetPlanesMembresiaInteractor getPlanes,
        IGetMetodosPagoInteractor getMetodosPago,
        ICreatePagoInteractor createPago,
        IValidationUIService validationUIService,
        IGetSociosLookupInteractor? sociosLookup = null
    )
    {
        _getPlanes = getPlanes;
        _getMetodosPago = getMetodosPago;
        _createPago = createPago;
        _validation = validationUIService;
        _sociosLookup = sociosLookup;

        _ = CargarCombosAsync();

        BindBusyToCommands(GuardarCommand, CancelarCommand);
    }

    public void Initialize(SocioLookupDto? socio)
    {
        if (socio is null)
        {
            ShowSocioPicker = true;
            SocioSeleccionado = null;
            Titulo = "Registrar Pago (Seleccioná un socio)";
        }
        else
        {
            ShowSocioPicker = false;
            SocioSeleccionado = socio;
            Titulo = $"Registrar Pago para {socio.NombreCompleto} (DNI {socio.Dni})";
        }

        OnPropertyChanged(nameof(SocioResumen));
        OnPropertyChanged(nameof(FormularioValido));
        GuardarCommand.NotifyCanExecuteChanged();
    }

    private async Task CargarCombosAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var planesReq = new GetPlanesRequest
            {
                Status = StatusFilter.Activo,
                PageNumber = 1,
                PageSize = 500
            };
            var planes = await _getPlanes.ExecuteAsync(planesReq, _cts.Token);
            Planes.Clear();
            foreach (var p in planes.Items)
                Planes.Add(p);
            
            PlanSeleccionado = Planes.FirstOrDefault();

            var request = new GetMetodoPagoRequest
            {
                PageNumber = 1,
                PageSize = 500
            };
            var result = await _getMetodosPago.ExecuteAsync(request, _cts.Token);

            MetodosPago.Clear();
            foreach (var metodo in result.Items) MetodosPago.Add(metodo);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar datos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DebouncedBuscarSociosAsync(int delayMs = 280)
    {
        if (_sociosLookup is null) return;

        _searchCts?.Cancel();
        var cts = new CancellationTokenSource();
        _searchCts = cts;

        try
        {
            await Task.Delay(delayMs, cts.Token);
            var texto = SocioBusqueda?.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                SociosSugeridos.Clear();
                return;
            }

            var resultados = await _sociosLookup.ExecuteAsync(texto, cts.Token);
            SociosSugeridos.Clear();
            foreach (var s in resultados) SociosSugeridos.Add(s);
        }
        catch (OperationCanceledException) { }
    }


    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(SocioSeleccionado) => !HasSocioInteracted ? string.Empty
            : SocioSeleccionado is null ? "Seleccioná un socio." : string.Empty,

        nameof(PlanSeleccionado) => !HasPlanInteracted ? string.Empty
            : PlanSeleccionado is null ? "Seleccioná un plan." : string.Empty,

        nameof(MetodoSeleccionado) => !HasMetodoInteracted ? string.Empty
            : MetodoSeleccionado is null ? "Seleccioná un método de pago." : string.Empty,

        nameof(Precio) => !HasPrecioInteracted ? string.Empty
            : (!decimal.TryParse(Precio, NumberStyles.Number, CultureInfo.InvariantCulture, out var monto) || monto < 0
               ? "Precio debe ser un número válido ≥ 0." : string.Empty),

        nameof(FechaPago) => !HasFechaPagoInteracted ? string.Empty
            : FechaPago is null ? "Seleccioná la fecha de pago." : string.Empty,

        nameof(FechaVencimiento) => !HasVencimientoInteracted ? string.Empty
            : FechaVencimiento is null ? "Seleccioná la fecha de vencimiento."
            : (FechaPago is not null && FechaVencimiento < FechaPago ? "El vencimiento no puede ser anterior al pago." : string.Empty),

        _ => string.Empty
    };

    public bool FormularioValido =>
        string.IsNullOrEmpty(this[nameof(SocioSeleccionado)]) &&
        string.IsNullOrEmpty(this[nameof(PlanSeleccionado)]) &&
        string.IsNullOrEmpty(this[nameof(MetodoSeleccionado)]) &&
        string.IsNullOrEmpty(this[nameof(Precio)]) &&
        string.IsNullOrEmpty(this[nameof(FechaPago)]) &&
        string.IsNullOrEmpty(this[nameof(FechaVencimiento)]) &&
        CreditosAsignados > 0;

    // --------- Commands ----------
    [RelayCommand(CanExecute = nameof(CanGuardar))]
    private async Task GuardarAsync()
    {
        HasSocioInteracted = HasPlanInteracted = HasMetodoInteracted =
            HasPrecioInteracted = HasFechaPagoInteracted = HasVencimientoInteracted = true;

        OnPropertyChanged(nameof(SocioSeleccionado));
        OnPropertyChanged(nameof(PlanSeleccionado));
        OnPropertyChanged(nameof(MetodoSeleccionado));
        OnPropertyChanged(nameof(Precio));
        OnPropertyChanged(nameof(FechaPago));
        OnPropertyChanged(nameof(FechaVencimiento));
        OnPropertyChanged(nameof(FormularioValido));

        if (!FormularioValido) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var fechaPagoLocal = DateTime.SpecifyKind(FechaPago!.Value, DateTimeKind.Local);
            var dto = new PagoCreateDto
            {
                SocioId = SocioSeleccionado!.Id,
                PlanMembresiaId = PlanSeleccionado!.Id,
                Precio = decimal.Parse(Precio, CultureInfo.InvariantCulture),
                FechaPago = fechaPagoLocal,
                MetodoPagoId = MetodoSeleccionado!.Id,
                Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones.Trim(),
                CreditosAsignados = CreditosAsignados,
                FechaVencimiento = FechaVencimiento!.Value.Date.AddHours(23).AddMinutes(59),
                TipoAjusteAplicado = MetodoSeleccionado!.TipoAjuste,
                ValorAjusteAplicado = MetodoSeleccionado!.ValorAjuste,
                MontoFinal = MontoFinal!.Value,
                AjusteImporte = Delta!.Value
            };

            Result result = await _createPago.ExecuteAsync(dto, _cts.Token);
            if (result.IsSuccess)
                RequestClose();
            else
                ErrorMessage = string.Join(Environment.NewLine, result.Errors);
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

    private bool CanGuardar() => !IsBusy && FormularioValido;

    [RelayCommand] private void Cancelar() => RequestClose();


    [RelayCommand]
    private void LimpiarSocio()
    {
        SocioBusqueda = string.Empty;
        SocioSeleccionado = null;
        SociosSugeridos.Clear();
        ShowSocioPicker = true;
        HasSocioInteracted = true;
        OnPropertyChanged(nameof(SocioResumen));
        OnPropertyChanged(nameof(FormularioValido));
        GuardarCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _searchCts?.Cancel();
        _searchCts?.Dispose();
    }
}
