using System.Xml;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// SERVICIO ORQUESTADOR (Facade Pattern)
/// ═══════════════════════════════════════════════════════════════
/// Simplifica el uso del storage para el resto de la aplicación
/// </summary>
public interface IServicioAlmacenamientoDocumentos
{
    // ═══════════════════════════════════════════════════════════════
    // MÉTODO ANTERIOR (mantenido por compatibilidad)
    // ═══════════════════════════════════════════════════════════════
    Task<DocumentoAlmacenado> GuardarDocumentoAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        XmlDocument xmlFirmado,
        string? proveedor = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ═══════════════════════════════════════════════════════════════
    /// MÉTODO NUEVO - Guarda el XML firmado usando bytes exactos
    /// ═══════════════════════════════════════════════════════════════
    /// Este método evita problemas de re-serialización que invalidan
    /// la firma digital al preservar los bytes exactos del XML firmado.
    /// </summary>
    /// <param name="clave">Clave del documento</param>
    /// <param name="xmlSinFirmar">XML sin firmar (puede ser XmlDocument)</param>
    /// <param name="bytesXmlFirmado">Bytes EXACTOS del XML firmado - NO re-serializar</param>
    /// <param name="xmlFirmadoParaMetadata">XmlDocument solo para extraer metadata</param>
    /// <param name="proveedor">Proveedor de storage</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task<DocumentoAlmacenado> GuardarDocumentoConBytesAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        byte[] bytesXmlFirmado,
        XmlDocument xmlFirmadoParaMetadata,
        string? proveedor = null,
        CancellationToken cancellationToken = default);

    Task ActualizarConRespuestaAsync(
        string clave,
        string xmlRespuesta,
        CancellationToken cancellationToken = default);

    Task<DocumentoAlmacenado?> ObtenerDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default);
}