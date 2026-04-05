using System.Security.Cryptography;
using System.Text;
using FacturaElectronica.Dominio.Abstracciones.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Infraestructura.Services;

/// <summary>
/// Servicio de encriptación AES-256 para datos sensibles.
/// Utiliza las claves configuradas en appsettings.json (Security:EncryptionKey/EncryptionIV).
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly ILogger<AesEncryptionService> _logger;

    public AesEncryptionService(
        IConfiguration configuration,
        ILogger<AesEncryptionService> logger)
    {
        _logger = logger;

        var keyBase64 = configuration["Security:EncryptionKey"];
        var ivBase64 = configuration["Security:EncryptionIV"];

        if (string.IsNullOrEmpty(keyBase64) || string.IsNullOrEmpty(ivBase64))
        {
            _logger.LogWarning(
                "Security:EncryptionKey o Security:EncryptionIV no están configurados. " +
                "Las claves de certificados no podrán ser desencriptadas.");

            // Usar valores por defecto para evitar errores (solo en desarrollo)
            _key = new byte[32];
            _iv = new byte[16];
            return;
        }

        try
        {
            _key = Convert.FromBase64String(keyBase64);
            _iv = Convert.FromBase64String(ivBase64);

            if (_key.Length != 32)
                throw new ArgumentException("EncryptionKey debe ser de 32 bytes (256 bits)");

            if (_iv.Length != 16)
                throw new ArgumentException("EncryptionIV debe ser de 16 bytes (128 bits)");
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Error al decodificar las claves de encriptación desde Base64");
            throw new InvalidOperationException(
                "Las claves de encriptación no están en formato Base64 válido", ex);
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "Error al desencriptar datos. Posible clave incorrecta.");
            throw new InvalidOperationException(
                "No se pudo desencriptar los datos. Verifique las claves de encriptación.", ex);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "El texto encriptado no está en formato Base64 válido");
            throw new InvalidOperationException(
                "El texto encriptado no está en formato válido.", ex);
        }
    }
}
