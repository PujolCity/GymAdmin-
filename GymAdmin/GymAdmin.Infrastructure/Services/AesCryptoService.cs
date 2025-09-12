using GymAdmin.Domain.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GymAdmin.Infrastructure.Services;

public class AesCryptoService : ICryptoService
{
    private readonly byte[] _key;
    private readonly string _secretFilePath;

    public AesCryptoService()
    {
        _secretFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GymAdmin",
            "secret.json");

        Directory.CreateDirectory(Path.GetDirectoryName(_secretFilePath)!);

        if (!File.Exists(_secretFilePath))
            GenerateAndSaveSecret();

        _key = LoadSecret();
    }

    private void GenerateAndSaveSecret()
    {
        using var rng = RandomNumberGenerator.Create();
        var key = new byte[32]; // 256 bits
        rng.GetBytes(key);

        var secretJson = new { SecretKey = Convert.ToBase64String(key) };
        File.WriteAllText(_secretFilePath, JsonSerializer.Serialize(secretJson));
    }

    private byte[] LoadSecret()
    {
        var json = File.ReadAllText(_secretFilePath);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var keyBase64 = doc.GetProperty("SecretKey").GetString()!;
        return Convert.FromBase64String(keyBase64);
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public string ComputeHash(string plainText)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}