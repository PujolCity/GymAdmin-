namespace GymAdmin.Applications.DTOs.Asistencia;

public class AsistenciaDto
{
    public int Id {  get; set; }
    public int SocioId { get; set; }
    public DateTime Entrada { get; set; }
    public bool SeUsoCredito { get; set; }
    public string Observaciones { get; set; } = string.Empty;

}
