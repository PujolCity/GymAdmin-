namespace GymAdmin.Applications.DTOs.PagosDto;

public class PagoDto
{
    public int Id { get; set; }
    public int SocioId { get; set; }
    public string SocioNombre { get; set; }
    public int PlanMembresiaId { get; set; }
    public string PlanNombre { get; set; }
    public decimal Precio { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public int MetodoPagoId { get; set; }
    public string MetodoPago { get; set; } 
    public string Observaciones { get; set; }
    public string Estado { get; set; }
    public int CreditosAsignados { get; set; }
    public DateTime FechaVencimiento { get; set; }
}
