using System.Xml;
using Amazon.S3;
using Amazon.S3.Model;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.S3;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// PROVEEDOR: AWS S3
/// ═══════════════════════════════════════════════════════════════
/// NuGet: AWSSDK.S3
/// </summary>
public class AwsS3StorageProvider : StorageProviderBase
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public override string ProviderName => "AWS_S3";

    public AwsS3StorageProvider(
        IAmazonS3 s3Client,
        ILogger<AwsS3StorageProvider> logger,
        IOptions<StorageOptions> options)
        : base(logger, options.Value)
    {
        _s3Client = s3Client;
        _bucketName = options.Value.RutaBase; // En S3, esto sería el nombre del bucket
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
            Logger.LogDebug("Guardando documento {Clave} en AWS S3", clave);

            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var prefijo = $"{año}/{mes:D2}/{dia:D2}/{clave}";

            // 1. Subir XML sin firmar
            var keySinFirmar = $"{prefijo}/01-sin-firmar.xml";
            await SubirXmlAsync(keySinFirmar, xmlSinFirmar, cancellationToken);

            // 2. Subir XML firmado
            var keyFirmado = $"{prefijo}/02-firmado.xml";
            await SubirXmlAsync(keyFirmado, xmlFirmado, cancellationToken);

            // 3. Subir respuesta (si existe)
            string? keyRespuesta = null;
            if (!string.IsNullOrEmpty(xmlRespuesta))
            {
                keyRespuesta = $"{prefijo}/03-respuesta.xml";
                await SubirTextoAsync(keyRespuesta, xmlRespuesta, cancellationToken);
            }

            // 4. Subir metadata
            var metadata = GenerarMetadata(clave, xmlFirmado);
            var keyMetadata = $"{prefijo}/metadata.json";
            await SubirMetadataAsync(keyMetadata, metadata, cancellationToken);

            Logger.LogInformation("Documento {Clave} guardado en S3 bucket {Bucket}", clave, _bucketName);

            return new DocumentoAlmacenado
            {
                Clave = clave,
                FechaCreacion = DateTime.Now,
                XmlSinFirmar = XmlToString(xmlSinFirmar),
                XmlFirmado = XmlToString(xmlFirmado),
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = $"s3://{_bucketName}/{keySinFirmar}",
                RutaFirmado = $"s3://{_bucketName}/{keyFirmado}",
                RutaRespuesta = keyRespuesta != null ? $"s3://{_bucketName}/{keyRespuesta}" : null,
                ProveedorStorage = ProviderName,
                MetadataAdicional = metadata
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando documento {Clave} en S3", clave);
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
            Logger.LogDebug("Guardando documento {Clave} en AWS S3 (con bytes preservados)", clave);

            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var prefijo = $"{año}/{mes:D2}/{dia:D2}/{clave}";

            // 1. Subir XML sin firmar
            var keySinFirmar = $"{prefijo}/01-sin-firmar.xml";
            await SubirXmlAsync(keySinFirmar, xmlSinFirmar, cancellationToken);

            // 2. Subir XML firmado - USANDO BYTES EXACTOS
            var keyFirmado = $"{prefijo}/02-firmado.xml";
            await SubirBytesAsync(keyFirmado, bytesXmlFirmado, cancellationToken);

            // 3. Subir respuesta (si existe)
            string? keyRespuesta = null;
            if (!string.IsNullOrEmpty(xmlRespuesta))
            {
                keyRespuesta = $"{prefijo}/03-respuesta.xml";
                await SubirTextoAsync(keyRespuesta, xmlRespuesta, cancellationToken);
            }

            // 4. Subir metadata
            var metadata = GenerarMetadata(clave, xmlFirmadoParaMetadata);
            var keyMetadata = $"{prefijo}/metadata.json";
            await SubirMetadataAsync(keyMetadata, metadata, cancellationToken);

            Logger.LogInformation("Documento {Clave} guardado en S3 bucket {Bucket} (bytes preservados)", clave, _bucketName);

            return new DocumentoAlmacenado
            {
                Clave = clave,
                FechaCreacion = DateTime.Now,
                XmlSinFirmar = XmlToString(xmlSinFirmar),
                XmlFirmado = System.Text.Encoding.UTF8.GetString(bytesXmlFirmado),
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = $"s3://{_bucketName}/{keySinFirmar}",
                RutaFirmado = $"s3://{_bucketName}/{keyFirmado}",
                RutaRespuesta = keyRespuesta != null ? $"s3://{_bucketName}/{keyRespuesta}" : null,
                ProveedorStorage = ProviderName,
                MetadataAdicional = metadata
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando documento {Clave} en S3", clave);
            throw;
        }
    }

    public override async Task ActualizarRespuestaAsync(
        string clave,
        string xmlRespuesta,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var key = $"{año}/{mes:D2}/{dia:D2}/{clave}/03-respuesta.xml";

            await SubirTextoAsync(key, xmlRespuesta, cancellationToken);

            Logger.LogInformation("Respuesta actualizada en S3 para {Clave}", clave);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error actualizando respuesta en S3 para {Clave}", clave);
            throw;
        }
    }

    public override async Task<DocumentoAlmacenado?> ObtenerDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var prefijo = $"{año}/{mes:D2}/{dia:D2}/{clave}";

            var keySinFirmar = $"{prefijo}/01-sin-firmar.xml";
            var keyFirmado = $"{prefijo}/02-firmado.xml";
            var keyRespuesta = $"{prefijo}/03-respuesta.xml";

            var xmlSinFirmar = await DescargarTextoAsync(keySinFirmar, cancellationToken);
            var xmlFirmado = await DescargarTextoAsync(keyFirmado, cancellationToken);
            var xmlRespuesta = await DescargarTextoAsync(keyRespuesta, cancellationToken, opcional: true);

            if (xmlSinFirmar == null || xmlFirmado == null)
                return null;

            return new DocumentoAlmacenado
            {
                Clave = clave,
                XmlSinFirmar = xmlSinFirmar,
                XmlFirmado = xmlFirmado,
                XmlRespuesta = xmlRespuesta,
                RutaSinFirmar = $"s3://{_bucketName}/{keySinFirmar}",
                RutaFirmado = $"s3://{_bucketName}/{keyFirmado}",
                RutaRespuesta = xmlRespuesta != null ? $"s3://{_bucketName}/{keyRespuesta}" : null,
                ProveedorStorage = ProviderName
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error obteniendo documento {Clave} de S3", clave);
            throw;
        }
    }

    public override async Task<bool> EliminarDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var prefijo = $"{año}/{mes:D2}/{dia:D2}/{clave}";

            // Listar todos los objetos con ese prefijo
            var listRequest = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefijo
            };

            var response = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);

            // Eliminar cada objeto
            foreach (var obj in response.S3Objects)
            {
                await _s3Client.DeleteObjectAsync(_bucketName, obj.Key, cancellationToken);
            }

            Logger.LogInformation("Documento {Clave} eliminado de S3", clave);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error eliminando documento {Clave} de S3", clave);
            return false;
        }
    }

    public override async Task<bool> ExisteDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (año, mes, dia) = ExtraerFechaDeClave(clave);
            var key = $"{año}/{mes:D2}/{dia:D2}/{clave}/02-firmado.xml";

            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public override async Task<List<DocumentoAlmacenado>> ListarDocumentosAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        int limite = 100,
        CancellationToken cancellationToken = default)
    {
        var documentos = new List<DocumentoAlmacenado>();

        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                MaxKeys = limite
            };

            var response = await _s3Client.ListObjectsV2Async(request, cancellationToken);

            // Filtrar y mapear resultados
            // (Implementación simplificada)

            return documentos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error listando documentos en S3");
            throw;
        }
    }

    public override async Task<EstadisticasStorage> ObtenerEstadisticasAsync(
        CancellationToken cancellationToken = default)
    {
        // Implementar estadísticas consultando S3
        return new EstadisticasStorage();
    }

    // Métodos auxiliares
    private async Task SubirXmlAsync(string key, XmlDocument xml, CancellationToken cancellationToken)
    {
        var xmlString = XmlToString(xml);
        await SubirTextoAsync(key, xmlString, cancellationToken);
    }

    private async Task SubirTextoAsync(string key, string contenido, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            ContentBody = contenido,
            ContentType = key.EndsWith(".xml") ? "application/xml" : "application/json"
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }

    private async Task SubirMetadataAsync(string key, Dictionary<string, string> metadata, CancellationToken cancellationToken)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(metadata);
        await SubirTextoAsync(key, json, cancellationToken);
    }

    private async Task<string?> DescargarTextoAsync(string key, CancellationToken cancellationToken, bool opcional = false)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }
        catch (AmazonS3Exception ex) when (opcional && ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private async Task SubirBytesAsync(string key, byte[] bytes, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(bytes);
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = "application/xml"
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }
}