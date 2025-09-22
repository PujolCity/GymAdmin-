namespace GymAdmin.Domain.Entities;

public class Asistencia : EntityBase
{
    public int SocioId { get; set; }
    public Socio Socio { get; set; }
    public DateTime Entrada { get; set; } = DateTime.UtcNow;
    public DateTime? Salida { get; set; }

    public bool SeUsoCredito { get; set; }
    public string Observaciones { get; set; } = string.Empty;

    // Método para registrar asistencia
    public static Asistencia RegistrarAsistencia(Socio socio, string observaciones = "")
    {
        var asistencia = new Asistencia
        {
            SocioId = socio.Id,
            Entrada = DateTime.UtcNow,
            SeUsoCredito = socio.UsarCredito(), 
            Observaciones = observaciones
        };

        return asistencia;
    }
}