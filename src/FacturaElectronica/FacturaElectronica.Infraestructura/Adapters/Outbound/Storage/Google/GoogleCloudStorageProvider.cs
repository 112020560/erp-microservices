using System.Text;
using System.Xml;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;
using FacturaElectronica.Dominio.Settings;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.Google;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// PROVEEDOR: GOOGLE CLOUD STORAGE
/// ═══════════════════════════════════════════════════════════════
/// NuGet: Google.Cloud.Storage.V1
/// </summary>
public class GoogleCloudStorageProvider : StorageProviderBase
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public override string ProviderName => "Google_Cloud";

    public GoogleCloudStorageProvider(
        StorageClient storageClient,
        ILogger<GoogleCloudStorageProvider> logger,
        IOptions<StorageOptions> options)
        : base(logger, options.Value)
    {
        _storageClient = storageClient;
        _bucketName = options.Value.RutaBase;
    }

    public override async Task<DocumentoAlmacenado> GuardarDocumentoCompletoAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        XmlDocument xmlFirmado,
        string? xmlRespuesta,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Guardando documento {Clave} en Google Cloud Storage", clave);

            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var prefijo = $"{año}/{mes:D2}/{dia:D2}/{clave}";

            // Subir archivos usando Google Cloud Storage API
            var objectNameSinFirmar = $"{prefijo}/01-sin-firmar.xml";
            await SubirXmlAsync(objectNameSinFirmar, xmlSinFirmar, cancellationToken);

            var objectNameFirmado = $"{prefijo}/02-firmado.xml";
            await SubirXmlAsync(objectNameFirmado, xmlFirmado, cancellationToken);

            Logger.LogInformation("Documento {Clave} guardado en Google Cloud", clave);

            return new DocumentoAlmacenado
            {
                Clave = clave,
                FechaCreacion = DateTime.Now,
                XmlSinFirmar = XmlToString(xmlSinFirmar),
                XmlFirmado = XmlToString(xmlFirmado),
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = $"gs://{_bucketName}/{objectNameSinFirmar}",
                RutaFirmado = $"gs://{_bucketName}/{objectNameFirmado}",
                ProveedorStorage = ProviderName
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando documento {Clave} en Google Cloud", clave);
            throw;
        }
    }

    /// <summary>
    /// ═══════════════════════════════════════════════════════════════
    /// MÉTODO NUEVO - Guarda preservando los bytes exactos del XML firmado
    /// ═══════════════════════════════════════════════════════════════
    /// </summary>
    public override async Task<DocumentoAlmacenado> GuardarDocumentoCompletoConBytesAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        byte[] bytesXmlFirmado,
        XmlDocument xmlFirmadoParaMetadata,
        string? xmlRespuesta,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Guardando documento {Clave} en Google Cloud Storage (con bytes preservados)", clave);

            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var prefijo = $"{año}/{mes:D2}/{dia:D2}/{clave}";

            // Subir archivos usando Google Cloud Storage API
            var objectNameSinFirmar = $"{prefijo}/01-sin-firmar.xml";
            await SubirXmlAsync(objectNameSinFirmar, xmlSinFirmar, cancellationToken);

            // Subir XML firmado - USANDO BYTES EXACTOS
            var objectNameFirmado = $"{prefijo}/02-firmado.xml";
            await SubirBytesAsync(objectNameFirmado, bytesXmlFirmado, cancellationToken);

            Logger.LogInformation("Documento {Clave} guardado en Google Cloud (bytes preservados)", clave);

            return new DocumentoAlmacenado
            {
                Clave = clave,
                FechaCreacion = DateTime.Now,
                XmlSinFirmar = XmlToString(xmlSinFirmar),
                XmlFirmado = Encoding.UTF8.GetString(bytesXmlFirmado),
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = $"gs://{_bucketName}/{objectNameSinFirmar}",
                RutaFirmado = $"gs://{_bucketName}/{objectNameFirmado}",
                ProveedorStorage = ProviderName
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando documento {Clave} en Google Cloud", clave);
            throw;
        }
    }

    // Implementar resto de métodos...
    public override Task ActualizarRespuestaAsync(string clave, string xmlRespuesta, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<DocumentoAlmacenado?> ObtenerDocumentoAsync(string clave, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> EliminarDocumentoAsync(string clave, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> ExisteDocumentoAsync(string clave, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<List<DocumentoAlmacenado>> ListarDocumentosAsync(DateTime fechaDesde, DateTime fechaHasta, int limite = 100, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<EstadisticasStorage> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task SubirXmlAsync(string objectName, XmlDocument xml, CancellationToken cancellationToken)
    {
        var xmlString = XmlToString(xml);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
        await _storageClient.UploadObjectAsync(
            _bucketName,
            objectName,
            "application/xml",
            stream,
            cancellationToken: cancellationToken);
    }

    private async Task SubirBytesAsync(string objectName, byte[] bytes, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(bytes);
        await _storageClient.UploadObjectAsync(
            _bucketName,
            objectName,
            "application/xml",
            stream,
            cancellationToken: cancellationToken);
    }
}