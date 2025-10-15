using GymAdmin.Domain.Interfaces.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymAdmin.Domain.Entities;

public class SystemConfig : EntityBase, IEncryptableEntity
{
    public string NombreGimnasio { get; set; }
    public string EmailContacto { get; set; }
    public string Direccion { get; set; }

    public string TelefonoEncrypted { get; set; }
    [NotMapped] public string Telefono { get; set; }

    public string? CarpetaBase { get; set; }        
    public string? CarpetaBackups { get; set; }     
    public DateTime? UltimoBackupAt { get; set; }

    public string PrefijoArchivos { get; set; } = "GymAdmin_";
    public bool IncluirNombreEnExport { get; set; } = true;

    // --- Opcionales útiles ---
    public string WhatsAppEncrypted { get; set; }
    [NotMapped] public string WhatsApp { get; set; }

    public string CuitEncrypted { get; set; }
    [NotMapped] public string Cuit { get; set; }

    public int? BackupRetentionCount { get; set; }  

    // --- Hooks de cifrado ---
    public void HandleEncryption(ICryptoService crypto)
    {
        if (!string.IsNullOrWhiteSpace(Telefono))
            TelefonoEncrypted = crypto.Encrypt(Telefono);

        if (!string.IsNullOrWhiteSpace(WhatsApp))
            WhatsAppEncrypted = crypto.Encrypt(WhatsApp);

        if (!string.IsNullOrWhiteSpace(Cuit))
            CuitEncrypted = crypto.Encrypt(Cuit);
    }

    public void HandleDecryption(ICryptoService crypto)
    {
        if (!string.IsNullOrWhiteSpace(TelefonoEncrypted))
            Telefono = crypto.Decrypt(TelefonoEncrypted);

        if (!string.IsNullOrWhiteSpace(WhatsAppEncrypted))
            WhatsApp = crypto.Decrypt(WhatsAppEncrypted);

        if (!string.IsNullOrWhiteSpace(CuitEncrypted))
            Cuit = crypto.Decrypt(CuitEncrypted);
    }
}