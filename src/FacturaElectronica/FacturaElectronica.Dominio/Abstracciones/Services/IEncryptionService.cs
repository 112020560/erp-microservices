namespace FacturaElectronica.Dominio.Abstracciones.Services;

/// <summary>
/// Servicio de encriptación para datos sensibles.
/// Usado para encriptar/desencriptar claves de certificados almacenadas en BD.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encripta un texto plano.
    /// </summary>
    /// <param name="plainText">Texto a encriptar</param>
    /// <returns>Texto encriptado en Base64</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Desencripta un texto encriptado.
    /// </summary>
    /// <param name="encryptedText">Texto encriptado en Base64</param>
    /// <returns>Texto plano original</returns>
    string Decrypt(string encryptedText);
}
