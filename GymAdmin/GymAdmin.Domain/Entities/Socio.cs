using GymAdmin.Domain.Interfaces.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymAdmin.Domain.Entities;

public class Socio : EntityBase, IEncryptableEntity
{
    public string DniEncrypted { get; set; } = string.Empty;
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    //public EstadoSocio Estado { get; set; } = EstadoSocio.Inactivo;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public string DniHash { get; set; } = null!;
    public int CreditosRestantes { get; set; }
    public int TotalCreditosComprados { get; set; }
    //public DateTime UltimoPago { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiracionMembresia { get; set; } 

    [NotMapped] public string Dni { get; set; }
    [NotMapped] public DateTime? UltimaAsistencia { get; set; }

    [NotMapped]
    public string VigenciaTexto
    {
        get
        {
            if (ExpiracionMembresia == null)
                return "Sin registro";

            return ExpiracionMembresia.Value.Date < DateTime.Today
                ? $"Venció: {ExpiracionMembresia:dd/MM/yyyy}"
                : $"Vence: {ExpiracionMembresia:dd/MM/yyyy}";
        }
    }

    public ICollection<Pagos> Pagos { get; set; } = new List<Pagos>();
    public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();

  
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

    public void HandleDecryption(ICryptoService cryptoService)
    {
        if (!string.IsNullOrEmpty(DniEncrypted))
            Dni = cryptoService.Decrypt(DniEncrypted);
    }

    public void HandleEncryption(ICryptoService cryptoService)
    {
        if (!string.IsNullOrEmpty(Dni))
        {
            DniEncrypted = cryptoService.Encrypt(Dni);
            DniHash = cryptoService.ComputeHash(Dni);
        }
    }

    public bool IsMembresiaExpirada =>
       ExpiracionMembresia.HasValue && ExpiracionMembresia.Value <= DateTime.UtcNow;

    public void ExpireIfNeeded()
    {
        if (IsMembresiaExpirada && CreditosRestantes > 0)
            CreditosRestantes = 0;
    }

    public bool UsarCredito()
    {
        ExpireIfNeeded();

        if (IsMembresiaExpirada || CreditosRestantes <= 0)
            return false;

        CreditosRestantes--;
        return true;
    }
}