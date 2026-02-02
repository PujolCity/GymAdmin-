using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.PagosDto;
using GymAdmin.Domain.Enums;
using System.Globalization;

namespace GymAdmin.Desktop.ViewModels.Pagos;

public partial class VerPagoViewModel : ViewModelBase
{
    [ObservableProperty] private int id;
    [ObservableProperty] private int socioId;
    [ObservableProperty] private string socioNombre = string.Empty;
    [ObservableProperty] private int planMembresiaId;
    [ObservableProperty] private string planNombre = string.Empty;
    [ObservableProperty] private decimal precio;
    [ObservableProperty] private decimal montoFinal;
    [ObservableProperty] private DateTime fechaPagoLocal;
    [ObservableProperty] private int metodoPagoId;
    [ObservableProperty] private string metodoPago = string.Empty;
    [ObservableProperty] private string descuentosRecargos = string.Empty;
    [ObservableProperty] private string observaciones = string.Empty;
    [ObservableProperty] private string estado = string.Empty;
    [ObservableProperty] private int creditosAsignados;
    [ObservableProperty] private DateTime fechaVencimientoLocal;

    [RelayCommand] private void Cancelar() => RequestClose();

    public void Load(PagoDto dto)
    {
        Id = dto.Id;
        SocioId = dto.SocioId;
        SocioNombre = dto.SocioNombre ?? string.Empty;
        PlanMembresiaId = dto.PlanMembresiaId;
        PlanNombre = dto.PlanNombre ?? string.Empty;
        Precio = dto.Precio;
        MontoFinal = dto.MontoFinal; DescuentosRecargos = BuildAjusteTexto(dto);
        FechaPagoLocal = DateTime.SpecifyKind(dto.FechaPago, DateTimeKind.Utc).ToLocalTime();
        MetodoPagoId = dto.MetodoPagoId;
        MetodoPago = dto.MetodoPago ?? string.Empty;
        Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? "—" : dto.Observaciones;
        Estado = dto.Estado ?? "—";
        CreditosAsignados = dto.CreditosAsignados;
        FechaVencimientoLocal = dto.FechaVencimiento.Kind switch
        {
            DateTimeKind.Utc => dto.FechaVencimiento.ToLocalTime(),
            DateTimeKind.Local => dto.FechaVencimiento,
            _ => DateTime.SpecifyKind(dto.FechaVencimiento, DateTimeKind.Local)
        };
    }

    private static string BuildAjusteTexto(PagoDto dto)
    {
        if (dto.TipoAjusteAplicado == TipoAjusteSaldo.Ninguno || dto.AjusteImporte == 0)
            return "Sin descuentos ni recargos.";

        var importe = Math.Abs(dto.AjusteImporte).ToString("C", CultureInfo.CurrentCulture);

        var tipo = dto.TipoAjusteAplicado switch
        {
            TipoAjusteSaldo.Porcentaje => "porcentaje",
            TipoAjusteSaldo.MontoFijo => "monto fijo",
            _ => "ajuste"
        };

        return dto.AjusteImporte < 0
            ? $"Descuento aplicado ({tipo}): −{importe}"
            : $"Recargo aplicado ({tipo}): +{importe}";
    }
}