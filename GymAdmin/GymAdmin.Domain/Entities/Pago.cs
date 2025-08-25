namespace GymAdmin.Domain.Entities;

public class Pago : EntityBase
{
    public int MiembroId { get; set; }
    public Miembro Miembro { get; set; }
    public int PlanMembresiaId { get; set; }
    public PlanMembresia PlanMembresia { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string MetodoPago { get; set; } // "Efectivo", "Tarjeta", "Transferencia"
    public string Observaciones { get; set; }

    // Créditos asignados en este pago
    public int CreditosAsignados { get; set; }
    public DateTime FechaVencimiento { get; set; }
}