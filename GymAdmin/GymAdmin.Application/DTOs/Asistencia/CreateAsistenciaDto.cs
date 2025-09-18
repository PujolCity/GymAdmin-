namespace GymAdmin.Applications.DTOs.Asistencia;

public class CreateAsistenciaDto
{
    public int IdSocio { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
