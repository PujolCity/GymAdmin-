namespace GymAdmin.Domain.Entities;

public class Miembro : EntityBase
{
    // Datos personales
    public string Dni { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

   
    // Estado (calculado basado en créditos)
    public bool IsActive => CreditosRestantes > 0 && ExpiracionMembresia > DateTime.UtcNow;

    // Sistema de créditos
    public int CreditosRestantes { get; set; }
    public int TotalCreditosComprados { get; set; }
    public DateTime ExpiracionMembresia { get; set; }

    // Relaciones
    public ICollection<Pago> Payments { get; set; } = new List<Pago>();
    public ICollection<Asistencia> Attendances { get; set; } = new List<Asistencia>();

    // Métodos de negocio
    public void AddCredits(int credits, int validityDays)
    {
        CreditosRestantes += credits;
        TotalCreditosComprados += credits;

        if (ExpiracionMembresia < DateTime.UtcNow || validityDays > 0)
        {
            ExpiracionMembresia = DateTime.UtcNow.AddDays(validityDays);
        }
    }

    public bool UsarCredito()
    {
        if (CreditosRestantes <= 0 || !IsActive)
            return false;

        CreditosRestantes--;
        return true;
    }
}