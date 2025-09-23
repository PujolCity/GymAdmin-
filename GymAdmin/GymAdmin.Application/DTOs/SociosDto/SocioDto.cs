namespace GymAdmin.Applications.DTOs.SociosDto;

public class SocioDto
{
    public int Id { get; set; }
    public string Dni { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string NombreCompleto => $"{Apellido} {Nombre}";
    public string Estado { get; set; } = "Inactivo";
    public int CreditosRestantes { get; set; }
    public DateTime? ExpiracionMembresia { get; set; } = DateTime.UtcNow;
    public DateTime? UltimaAsistencia { get; set; } = DateTime.UtcNow;
    public string VigenciaTexto { get; set; }
    public string FechaRegistro { get; set; }
    public int TotalCreditosComprados { get; set; }
    public string UltimoPagoTexto { get; set; }
    public string PlanNombre { get; set; }
}
