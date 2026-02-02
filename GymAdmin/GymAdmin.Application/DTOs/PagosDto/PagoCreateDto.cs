using GymAdmin.Domain.Enums;

namespace GymAdmin.Applications.DTOs.PagosDto;

public sealed class PagoCreateDto
{
    public int SocioId { get; set; }
    public int PlanMembresiaId { get; set; }
    public int MetodoPagoId { get; set; }
    public string? Observaciones { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int CreditosAsignados { get; set; }
    public DateTime FechaPago { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public TipoAjusteSaldo TipoAjusteAplicado { get; set; } = TipoAjusteSaldo.Ninguno;
    public decimal ValorAjusteAplicado { get; set; } = 0.0m;
    public decimal AjusteImporte { get; set; } = 0m;
    public decimal MontoFinal { get; set; }
}