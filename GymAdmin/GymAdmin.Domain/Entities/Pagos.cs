using GymAdmin.Domain.Enums;

namespace GymAdmin.Domain.Entities;

public class Pagos : EntityBase
{
    public int SocioId { get; set; }
    public int PlanMembresiaId { get; set; }
    public decimal Precio { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; } = string.Empty;
    public EstadoPago Estado { get; set; } 

    // Créditos asignados en este pago
    public int CreditosAsignados { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int MetodoPagoId { get; set; }
    public MetodoPago MetodoPagoRef { get; set; } = null!;
    public Socio Socio { get; set; }
    public PlanesMembresia PlanMembresia { get; set; }

    // Snapshot del ajuste aplicado
    public TipoAjusteSaldo TipoAjusteAplicado { get; set; } = TipoAjusteSaldo.Ninguno;
    public decimal ValorAjusteAplicado { get; set; } = 0m;

    // Resultado del cálculo
    public decimal AjusteImporte { get; set; } = 0m;  // + o - según tu regla
    public decimal MontoFinal { get; set; }           // MontoBase + AjusteImporte

    bool IsAnulado => Estado == EstadoPago.Anulado;
}