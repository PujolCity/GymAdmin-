namespace GymAdmin.Domain.Entities;

public class SystemConfig : EntityBase
{
    public string NombreGimnasio { get; set; }
    public string Direccion { get; set; }
    public string Telefono { get; set; }
    public int DiasValidezCredito { get; set; } = 30;
    public bool ExpiracionAutomaticaCredito { get; set; } = true;
}