using System.Xml;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// CONTRATO BASE - Strategy Pattern
/// ═══════════════════════════════════════════════════════════════
/// Interfaz que todos los proveedores de storage deben implementar
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Nombre del proveedor (para logging y selección)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Guarda un documento completo (sin firmar, firmado, respuesta)
    /// </summary>
    Task<DocumentoAlmacenado> GuardarDocumentoCompletoAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        XmlDocument xmlFirmado,
        string? xmlRespuesta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ═══════════════════════════════════════════════════════════════
    /// MÉTODO NUEVO - Guarda preservando los bytes exactos del XML firmado
    /// ═══════════════════════════════════════════════════════════════
    /// Este método evita el problema de "XML modificado después de firmado"
    /// al escribir los bytes exactos sin re-serialización.
    /// </summary>
    /// <param name="clave">Clave del documento</param>
    /// <param name="xmlSinFirmar">XML sin firmar</param>
    /// <param name="bytesXmlFirmado">Bytes EXACTOS del XML firmado</param>
    /// <param name="xmlFirmadoParaMetadata">XmlDocument solo para metadata</param>
    /// <param name="xmlRespuesta">Respuesta de Hacienda (si existe)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task<DocumentoAlmacenado> GuardarDocumentoCompletoConBytesAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        byte[] bytesXmlFirmado,
        XmlDocument xmlFirmadoParaMetadata,
        string? xmlRespuesta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza solo la respuesta de Hacienda
    /// </summary>
    Task ActualizarRespuestaAsync(
        string clave,
        string xmlRespuesta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un documento por su clave
    /// </summary>
    Task<DocumentoAlmacenado?> ObtenerDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un documento (soft delete recomendado)
    /// </summary>
    Task<bool> EliminarDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un documento
    /// </summary>
    Task<bool> ExisteDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista documentos por rango de fechas
    /// </summary>
    Task<List<DocumentoAlmacenado>> ListarDocumentosAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        int limite = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de almacenamiento
    /// </summary>
    Task<EstadisticasStorage> ObtenerEstadisticasAsync(
        CancellationToken cancellationToken = default);
}