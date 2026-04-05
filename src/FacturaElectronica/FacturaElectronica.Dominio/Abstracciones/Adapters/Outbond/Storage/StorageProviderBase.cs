using System.Text;
using System.Xml;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage.Models;
using FacturaElectronica.Dominio.Settings;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;

/// <summary>
/// ═══════════════════════════════════════════════════════════════
/// CLASE BASE ABSTRACTA (opcional pero recomendada)
/// ═══════════════════════════════════════════════════════════════
/// Implementa lógica común para todos los proveedores
/// </summary>
public abstract class StorageProviderBase : IStorageProvider
{
    protected readonly ILogger Logger;
    protected readonly StorageOptions Options;

    protected StorageProviderBase(ILogger logger, StorageOptions options)
    {
        Logger = logger;
        Options = options;
    }

    public abstract string ProviderName { get; }

    public abstract Task<DocumentoAlmacenado> GuardarDocumentoCompletoAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        XmlDocument xmlFirmado,
        string? xmlRespuesta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ═══════════════════════════════════════════════════════════════
    /// MÉTODO NUEVO - Guarda preservando los bytes exactos del XML firmado
    /// ═══════════════════════════════════════════════════════════════
    /// </summary>
    public abstract Task<DocumentoAlmacenado> GuardarDocumentoCompletoConBytesAsync(
        string clave,
        XmlDocument xmlSinFirmar,
        byte[] bytesXmlFirmado,
        XmlDocument xmlFirmadoParaMetadata,
        string? xmlRespuesta,
        CancellationToken cancellationToken = default);

    public abstract Task ActualizarRespuestaAsync(
        string clave,
        string xmlRespuesta,
        CancellationToken cancellationToken = default);

    public abstract Task<DocumentoAlmacenado?> ObtenerDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default);

    public abstract Task<bool> EliminarDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default);

    public abstract Task<bool> ExisteDocumentoAsync(
        string clave,
        CancellationToken cancellationToken = default);

    public abstract Task<List<DocumentoAlmacenado>> ListarDocumentosAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        int limite = 100,
        CancellationToken cancellationToken = default);

    public abstract Task<EstadisticasStorage> ObtenerEstadisticasAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Método helper para extraer fecha de la clave
    /// </summary>
    protected (int año, int mes, int dia) ExtraerFechaDeClave(string clave)
    {
        // Clave: 506 DD MM AA ...
        var dia = int.Parse(clave.Substring(3, 2));
        var mes = int.Parse(clave.Substring(5, 2));
        var año = 2000 + int.Parse(clave.Substring(7, 2));
        
        return (año, mes, dia);
    }

    /// <summary>
    /// Convierte XmlDocument a string
    /// </summary>
    protected string XmlToString(XmlDocument xml)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            OmitXmlDeclaration = false
        });
        xml.WriteTo(xmlWriter);
        xmlWriter.Flush();
        return stringWriter.ToString();
    }

    /// <summary>
    /// Convierte string a XmlDocument
    /// </summary>
    protected XmlDocument StringToXml(string xmlString)
    {
        var xml = new XmlDocument();
        xml.LoadXml(xmlString);
        return xml;
    }

    /// <summary>
    /// Genera metadata del documento
    /// </summary>
    protected Dictionary<string, string> GenerarMetadata(string clave, XmlDocument xmlFirmado)
    {
        var (año, mes, dia) = ExtraerFechaDeClave(clave);
        
        return new Dictionary<string, string>
        {
            ["clave"] = clave,
            ["año"] = año.ToString(),
            ["mes"] = mes.ToString(),
            ["dia"] = dia.ToString(),
            ["timestamp"] = DateTime.UtcNow.ToString("O"),
            ["proveedor"] = ProviderName,
            ["version"] = "4.4"
        };
    }
}