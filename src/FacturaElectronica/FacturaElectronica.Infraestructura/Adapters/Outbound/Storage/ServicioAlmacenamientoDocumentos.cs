using System.Xml;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Storage;

public class ServicioAlmacenamientoDocumentos : IServicioAlmacenamientoDocumentos
{
    private readonly IStorageProviderFactory _factory;
    private readonly ILogger<ServicioAlmacenamientoDocumentos> _logger;

    public ServicioAlmacenamientoDocumentos(
        IStorageProviderFactory factory,
        ILogger<ServicioAlmacenamientoDocumentos> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════
    // MÉTODO ANTERIOR (mantenido por compatibilidad)
    // ═══════════════════════════════════════════════════════════════
    public async Task<DocumentoAlmacenado> GuardarDocumentoAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        XmlDocument xmlFirmado,
        string? proveedor = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = string.IsNullOrEmpty(proveedor)
                ? _factory.CrearProveedorPorDefecto()
                : _factory.CrearProveedor(proveedor);

            _logger.LogInformation(
                "Guardando documento {Clave} usando proveedor {Proveedor}",
                clave, provider.ProviderName);

            return await provider.GuardarDocumentoCompletoAsync(
                clave,
                xmlSinFirmar,
                xmlFirmado,
                null, // Respuesta viene después
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando documento {Clave}", clave);
            throw;
        }
    }

    /// <summary>
    /// ═══════════════════════════════════════════════════════════════
    /// MÉTODO NUEVO - Guarda preservando los bytes exactos del XML firmado
    /// ═══════════════════════════════════════════════════════════════
    /// Este método evita el problema de "XML modificado después de firmado"
    /// </summary>
    public async Task<DocumentoAlmacenado> GuardarDocumentoConBytesAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        byte[] bytesXmlFirmado,
        XmlDocument xmlFirmadoParaMetadata,
        string? proveedor = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = string.IsNullOrEmpty(proveedor)
                ? _factory.CrearProveedorPorDefecto()
                : _factory.CrearProveedor(proveedor);

            _logger.LogInformation(
                "Guardando documento {Clave} con bytes preservados usando proveedor {Proveedor}",
                clave, provider.ProviderName);

            return await provider.GuardarDocumentoCompletoConBytesAsync(
                clave,
                xmlSinFirmar,
                bytesXmlFirmado,
                xmlFirmadoParaMetadata,
                null, // Respuesta viene después
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando documento {Clave}", clave);
            throw;
        }
    }

    public async Task ActualizarConRespuestaAsync(
        string clave,
        string xmlRespuesta,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Usar el proveedor por defecto (o puedes buscar dónde está guardado)
            var provider = _factory.CrearProveedorPorDefecto();

            _logger.LogInformation(
                "Actualizando respuesta para documento {Clave}",
                clave);

            await provider.ActualizarRespuestaAsync(clave, xmlRespuesta, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando respuesta para {Clave}", clave);
            throw;
        }
    }

    public async Task<DocumentoAlmacenado?> ObtenerDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = _factory.CrearProveedorPorDefecto();
            return await provider.ObtenerDocumentoAsync(clave, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo documento {Clave}", clave);
            throw;
        }
    }
}