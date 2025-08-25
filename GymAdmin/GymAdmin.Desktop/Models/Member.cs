namespace GymAdmin.Desktop.Models;

public class Member
{
    public int Id { get; set; }
    public string DNI { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime VencimientoMembresia { get; set; }
    public string Status => VencimientoMembresia >= DateTime.Now ? "Activo" : "Inactivo";

    public string FullName => $"{Apellido}, {Nombre }";
}
