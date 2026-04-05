using System.Xml.Serialization;

namespace FacturaElectronica.Dominio.Modelos.Xml;

/// <summary>
/// Modelo raíz para la Nota de Crédito Electrónica v4.4
/// Estructura similar a FacturaElectronica pero con namespace y elemento raíz diferentes.
/// InformacionReferencia es OBLIGATORIA para Notas de Crédito.
/// </summary>
[XmlRoot("NotaCreditoElectronica", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica")]
public class NotaCreditoElectronicaXml
{
    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Xmlns { get; set; } = new XmlSerializerNamespaces();

    [XmlElement("Clave")]
    public string Clave { get; set; } = string.Empty;

    [XmlElement("ProveedorSistemas")]
    public string? ProveedorSistemas { get; set; }

    [XmlElement("CodigoActividadEmisor")]
    public string CodigoActividad { get; set; } = string.Empty;

    [XmlElement("NumeroConsecutivo")]
    public string NumeroConsecutivo { get; set; } = string.Empty;

    [XmlElement("FechaEmision")]
    public string FechaEmision { get; set; } = string.Empty;

    [XmlElement("Emisor")]
    public EmisorXml Emisor { get; set; } = new EmisorXml();

    [XmlElement("Receptor")]
    public ReceptorXml? Receptor { get; set; }

    [XmlElement("CondicionVenta")]
    public string CondicionVenta { get; set; } = string.Empty;

    [XmlElement("CondicionVentaOtros")]
    public string? CondicionVentaOtros { get; set; }

    [XmlElement("PlazoCredito")]
    public string? PlazoCredito { get; set; }

    /// <summary>
    /// Controla si PlazoCredito se serializa en el XML.
    /// Solo se incluye si tiene un valor válido.
    /// </summary>
    public bool ShouldSerializePlazoCredito() => !string.IsNullOrEmpty(PlazoCredito);

    [XmlElement("DetalleServicio")]
    public DetalleServicioXml DetalleServicio { get; set; } = new DetalleServicioXml();

    [XmlElement("OtrosCargos")]
    public OtrosCargosXml? OtrosCargos { get; set; }

    [XmlElement("ResumenFactura")]
    public ResumenFacturaXml ResumenFactura { get; set; } = new ResumenFacturaXml();

    /// <summary>
    /// InformacionReferencia es OBLIGATORIA para Notas de Crédito.
    /// Vincula al documento original que se está afectando.
    /// Usa InformacionReferenciaNotaCreditoXml con TipoDocIR (requerido por Hacienda v4.4).
    /// </summary>
    [XmlElement("InformacionReferencia")]
    public InformacionReferenciaNotaCreditoXml InformacionReferencia { get; set; } = new InformacionReferenciaNotaCreditoXml();

    [XmlElement("Otros")]
    public OtrosXml? Otros { get; set; }

    public NotaCreditoElectronicaXml()
    {
        // Namespaces para v4.4
        Xmlns.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
        Xmlns.Add("vc", "http://www.w3.org/2007/XMLSchema-versioning");
        Xmlns.Add("xs", "http://www.w3.org/2001/XMLSchema");
    }
}
