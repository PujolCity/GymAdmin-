using GymAdmin.Domain.Interfaces.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymAdmin.Domain.Entities;

public class Socio : EntityBase, IEncryptableEntity
{
    public string DniEncrypted { get; set; } = string.Empty;
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public string DniHash { get; set; } = null!;
    public int CreditosRestantes { get; set; }
    public int TotalCreditosComprados { get; set; }
    public DateTime? ExpiracionMembresia { get; set; } = DateTime.UtcNow.AddDays(-1);

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
    [NotMapped] public DateTime? UltimoPago { get; set; }
    [NotMapped] public string PlanNombre { get; set; }
    [NotMapped] public decimal PlanPrecio { get; set; }

    public ICollection<Pagos> Pagos { get; set; } = new List<Pagos>();
    public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();

  
    // Métodos de negocio
    public void AddCredits(int credits, int validityDays, DateTime? nowUtc = null)
    {
        CreditosRestantes += credits;
        TotalCreditosComprados += credits;

        if (validityDays <= 0) return;

        var now = nowUtc ?? DateTime.UtcNow;
        var baseDate = (ExpiracionMembresia.HasValue && ExpiracionMembresia.Value > now)
                       ? ExpiracionMembresia.Value
                       : now;

        ExpiracionMembresia = baseDate.AddDays(validityDays);
    }
    public void AplicaCompraReseteando(int credits, DateTime nuevaExpiracionUtc)
    {
        CreditosRestantes = credits;               
        TotalCreditosComprados += credits;          
        ExpiracionMembresia = nuevaExpiracionUtc;  
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