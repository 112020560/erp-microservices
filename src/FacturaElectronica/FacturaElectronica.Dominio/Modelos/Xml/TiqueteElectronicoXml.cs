using System.Xml.Serialization;

namespace FacturaElectronica.Dominio.Modelos.Xml;

/// <summary>
/// Modelo raíz para el Tiquete Electrónico v4.4
/// Estructura similar a FacturaElectronica pero con namespace y elemento raíz diferentes.
/// El receptor es opcional para tiquetes (típicamente B2C sin identificación).
/// </summary>
[XmlRoot("TiqueteElectronico", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico")]
public class TiqueteElectronicoXml
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

    /// <summary>
    /// Receptor es OPCIONAL en tiquetes electrónicos.
    /// Típicamente no se incluye para ventas B2C (consumidor final).
    /// </summary>
    [XmlElement("Receptor")]
    public ReceptorXml? Receptor { get; set; }

    [XmlElement("CondicionVenta")]
    public string CondicionVenta { get; set; } = string.Empty;

    [XmlElement("CondicionVentaOtros")]
    public string? CondicionVentaOtros { get; set; }

    [XmlElement("PlazoCredito")]
    public string? PlazoCredito { get; set; }

    [XmlElement("DetalleServicio")]
    public DetalleServicioXml DetalleServicio { get; set; } = new DetalleServicioXml();

    [XmlElement("OtrosCargos")]
    public OtrosCargosXml? OtrosCargos { get; set; }

    [XmlElement("ResumenFactura")]
    public ResumenFacturaXml ResumenFactura { get; set; } = new ResumenFacturaXml();

    [XmlElement("InformacionReferencia")]
    public InformacionReferenciaXml? InformacionReferencia { get; set; }

    [XmlElement("Otros")]
    public OtrosXml? Otros { get; set; }

    public TiqueteElectronicoXml()
    {
        // Namespaces para v4.4
        Xmlns.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
        Xmlns.Add("vc", "http://www.w3.org/2007/XMLSchema-versioning");
        Xmlns.Add("xs", "http://www.w3.org/2001/XMLSchema");
    }
}
