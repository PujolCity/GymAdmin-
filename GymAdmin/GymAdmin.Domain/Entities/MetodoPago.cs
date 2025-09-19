namespace GymAdmin.Domain.Entities;

public class MetodoPago : EntityBase
{
    public string Nombre { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Pagos> Pagos { get; set; } = new List<Pagos>();
}