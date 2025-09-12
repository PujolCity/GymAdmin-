namespace GymAdmin.Domain.Entities;

public class Pagos : EntityBase
{
    public int Id { get; set; }
    public int SocioId { get; set; }
    public int PlanMembresiaId { get; set; }
    public decimal Precio { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public string MetodoPago { get; set; } // "Efectivo", "Tarjeta", "Transferencia"
    public string Observaciones { get; set; }

    // Créditos asignados en este pago
    public int CreditosAsignados { get; set; }
    public DateTime FechaVencimiento { get; set; }

    public Socio Socio { get; set; }
    public PlanesMembresia PlanMembresia { get; set; }
}