using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymAdmin.Applications.DTOs.PagosDto;

namespace GymAdmin.Desktop.ViewModels.Pagos;

public partial class VerPagoViewModel : ViewModelBase
{
    [ObservableProperty] private int id;
    [ObservableProperty] private int socioId;
    [ObservableProperty] private string socioNombre = string.Empty;
    [ObservableProperty] private int planMembresiaId;
    [ObservableProperty] private string planNombre = string.Empty;
    [ObservableProperty] private decimal precio;
    [ObservableProperty] private DateTime fechaPagoLocal;
    [ObservableProperty] private int metodoPagoId;
    [ObservableProperty] private string metodoPago = string.Empty;
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
        FechaPagoLocal = DateTime.SpecifyKind(dto.FechaPago, DateTimeKind.Utc).ToLocalTime();
        MetodoPagoId = dto.MetodoPagoId;
        MetodoPago = dto.MetodoPago ?? string.Empty;
        Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? "—" : dto.Observaciones;
        Estado = dto.Estado ?? "—";
        CreditosAsignados = dto.CreditosAsignados;
        // Si tu backend guarda FechaVencimiento como local, no conviertas; si es UTC, convertí:
        FechaVencimientoLocal = DateTime.SpecifyKind(dto.FechaVencimiento, DateTimeKind.Utc).ToLocalTime();
    }
}