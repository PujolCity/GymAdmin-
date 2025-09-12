namespace GymAdmin.Domain.Interfaces.Services;

public interface IEncryptableEntity
{
    void HandleEncryption(ICryptoService cryptoService);
    void HandleDecryption(ICryptoService cryptoService);
}
