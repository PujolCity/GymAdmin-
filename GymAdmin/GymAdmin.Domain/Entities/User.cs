using GymAdmin.Domain.Interfaces.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymAdmin.Domain.Entities;

public class User : EntityBase, IEncryptableEntity
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string EmailEncrypted { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    [NotMapped] public string? Email { get; set; } = string.Empty;


    public void HandleDecryption(ICryptoService cryptoService)
    {
        if (!string.IsNullOrEmpty(EmailEncrypted))
            Email = cryptoService.Decrypt(EmailEncrypted);
    }

    public void HandleEncryption(ICryptoService cryptoService)
    {
        if (!string.IsNullOrEmpty(Email))
            EmailEncrypted = cryptoService.Encrypt(Email);
    }
}