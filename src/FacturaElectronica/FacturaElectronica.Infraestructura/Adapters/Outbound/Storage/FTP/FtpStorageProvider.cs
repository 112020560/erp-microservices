using System.Xml;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage.FTP;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// PROVEEDOR: FTP/SFTP (Ejemplo adicional)
/// ═══════════════════════════════════════════════════════════════
/// NuGet: FluentFTP o SSH.NET
/// </summary>
public class FtpStorageProvider : StorageProviderBase
{
    public override string ProviderName => "FTP";

    public FtpStorageProvider(
        ILogger<FtpStorageProvider> logger,
        IOptions<StorageOptions> options)
        : base(logger, options.Value)
    {
    }

    // Implementación pendiente...
    public override Task<DocumentoAlmacenado> GuardarDocumentoCompletoAsync(string clave, XmlDocument xmlSinFirmar, XmlDocument xmlFirmado, string? xmlRespuesta, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// ═══════════════════════════════════════════════════════════════
    /// MÉTODO NUEVO - Guarda preservando los bytes exactos del XML firmado
    /// ═══════════════════════════════════════════════════════════════
    /// </summary>
    public override Task<DocumentoAlmacenado> GuardarDocumentoCompletoConBytesAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        byte[] bytesXmlFirmado,
        XmlDocument xmlFirmadoParaMetadata,
        string? xmlRespuesta,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implementar guardado con bytes exactos cuando se implemente FTP
        throw new NotImplementedException();
    }

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
}