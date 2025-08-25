namespace GymAdmin.Domain.Entities;

public class Asistencia : EntityBase
{
    public int MiembroId { get; set; }
    public Miembro Miembro { get; set; }
    public DateTime Entrada { get; set; } = DateTime.UtcNow;
    public DateTime? Salida { get; set; }

    public bool SeUsoCredito { get; set; }
    public string Observaciones { get; set; } = string.Empty;

    // Método para registrar asistencia
    public static Asistencia RegistrarAsistencia(Miembro miembro, string observaciones = "")
    {
        var asistencia = new Asistencia
        {
            MiembroId = miembro.Id,
            Entrada = DateTime.UtcNow,
            SeUsoCredito = miembro.UsarCredito(), // Intenta usar un crédito
            Observaciones = observaciones
        };

        return asistencia;
    }
}