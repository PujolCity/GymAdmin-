namespace GymAdmin.Domain.Interfaces.Services;

public interface ICryptoService
{
    string Encrypt(string plain);
    string Decrypt(string cipherBase64);
    string ComputeHash(string plainText); // nuevo

}