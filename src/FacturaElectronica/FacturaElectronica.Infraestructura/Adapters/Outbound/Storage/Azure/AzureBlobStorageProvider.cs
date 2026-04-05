using System.Text;
using System.Xml;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.Azure;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// PROVEEDOR: AZURE BLOB STORAGE
/// ═══════════════════════════════════════════════════════════════
/// NuGet: Azure.Storage.Blobs
/// </summary>
public class AzureBlobStorageProvider : StorageProviderBase
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly BlobContainerClient _containerClient;

    public override string ProviderName => "Azure_Blob";

    public AzureBlobStorageProvider(
        BlobServiceClient blobServiceClient,
        ILogger<AzureBlobStorageProvider> logger,
        IOptions<StorageOptions> options)
        : base(logger, options.Value)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = options.Value.RutaBase.ToLower(); // Azure requiere lowercase
        _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        
        // Crear contenedor si no existe
        _containerClient.CreateIfNotExists();
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
            Logger.LogDebug("Guardando documento {Clave} en Azure Blob Storage", clave);

            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var prefijo = $"{año}/{mes:D2}/{dia:D2}/{clave}";

            // 1. Subir XML sin firmar
            var blobSinFirmar = _containerClient.GetBlobClient($"{prefijo}/01-sin-firmar.xml");
            await SubirXmlAsync(blobSinFirmar, xmlSinFirmar, cancellationToken);

            // 2. Subir XML firmado
            var blobFirmado = _containerClient.GetBlobClient($"{prefijo}/02-firmado.xml");
            await SubirXmlAsync(blobFirmado, xmlFirmado, cancellationToken);

            // 3. Subir respuesta (si existe)
            BlobClient? blobRespuesta = null;
            if (!string.IsNullOrEmpty(xmlRespuesta))
            {
                blobRespuesta = _containerClient.GetBlobClient($"{prefijo}/03-respuesta.xml");
                await SubirTextoAsync(blobRespuesta, xmlRespuesta, cancellationToken);
            }

            Logger.LogInformation("Documento {Clave} guardado en Azure", clave);

            return new DocumentoAlmacenado
            {
                Clave = clave,
                FechaCreacion = DateTime.Now,
                XmlSinFirmar = XmlToString(xmlSinFirmar),
                XmlFirmado = XmlToString(xmlFirmado),
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = blobSinFirmar.Uri.ToString(),
                RutaFirmado = blobFirmado.Uri.ToString(),
                RutaRespuesta = blobRespuesta?.Uri.ToString(),
                ProveedorStorage = ProviderName
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando documento {Clave} en Azure", clave);
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
            Logger.LogDebug("Guardando documento {Clave} en Azure Blob Storage (con bytes preservados)", clave);

            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var prefijo = $"{año}/{mes:D2}/{dia:D2}/{clave}";

            // 1. Subir XML sin firmar
            var blobSinFirmar = _containerClient.GetBlobClient($"{prefijo}/01-sin-firmar.xml");
            await SubirXmlAsync(blobSinFirmar, xmlSinFirmar, cancellationToken);

            // 2. Subir XML firmado - USANDO BYTES EXACTOS
            var blobFirmado = _containerClient.GetBlobClient($"{prefijo}/02-firmado.xml");
            await SubirBytesAsync(blobFirmado, bytesXmlFirmado, cancellationToken);

            // 3. Subir respuesta (si existe)
            BlobClient? blobRespuesta = null;
            if (!string.IsNullOrEmpty(xmlRespuesta))
            {
                blobRespuesta = _containerClient.GetBlobClient($"{prefijo}/03-respuesta.xml");
                await SubirTextoAsync(blobRespuesta, xmlRespuesta, cancellationToken);
            }

            Logger.LogInformation("Documento {Clave} guardado en Azure (bytes preservados)", clave);

            return new DocumentoAlmacenado
            {
                Clave = clave,
                FechaCreacion = DateTime.Now,
                XmlSinFirmar = XmlToString(xmlSinFirmar),
                XmlFirmado = Encoding.UTF8.GetString(bytesXmlFirmado),
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = blobSinFirmar.Uri.ToString(),
                RutaFirmado = blobFirmado.Uri.ToString(),
                RutaRespuesta = blobRespuesta?.Uri.ToString(),
                ProveedorStorage = ProviderName
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando documento {Clave} en Azure", clave);
            throw;
        }
    }

    // Implementar resto de métodos de manera similar...
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

    private async Task SubirXmlAsync(BlobClient blob, XmlDocument xml, CancellationToken cancellationToken)
    {
        var xmlString = XmlToString(xml);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
        await blob.UploadAsync(stream, new BlobUploadOptions 
        { 
            HttpHeaders = new BlobHttpHeaders { ContentType = "application/xml" }
        }, cancellationToken);
    }

    private async Task SubirTextoAsync(BlobClient blob, string contenido, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(contenido));
        await blob.UploadAsync(stream, true, cancellationToken);
    }

    private async Task SubirBytesAsync(BlobClient blob, byte[] bytes, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(bytes);
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "application/xml" }
        }, cancellationToken);
    }
}
