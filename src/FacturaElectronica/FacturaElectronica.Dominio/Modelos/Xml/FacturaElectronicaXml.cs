using System.Xml.Serialization;

namespace FacturaElectronica.Dominio.Modelos.Xml;

/// <summary>
/// Modelo raíz para la Factura Electrónica v4.4
/// </summary>
// ═══════════════════════════════════════════════════════════════
// NAMESPACE ANTERIOR (incorrecto para v4.4):
// [XmlRoot("FacturaElectronica", Namespace = "https://tribunet.hacienda.go.cr/docs/esquemas/2017/v4.4/facturaElectronica")]
// ═══════════════════════════════════════════════════════════════
// NAMESPACE CORRECTO para v4.4 (cdn.comprobanteselectronicos.go.cr):
[XmlRoot("FacturaElectronica", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica")]
public class FacturaElectronicaXml
{
    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Xmlns { get; set; } = new XmlSerializerNamespaces();

    [XmlElement("Clave")]
    public string Clave { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════
    // NUEVO en v4.4: ProveedorSistemas es requerido y va después de Clave
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("ProveedorSistemas")]
    public string? ProveedorSistemas { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4 el elemento se llama "CodigoActividadEmisor" (no "CodigoActividad")
    // ═══════════════════════════════════════════════════════════════
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

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: CondicionVentaOtros es opcional (solo cuando CondicionVenta="99")
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("CondicionVentaOtros")]
    public string? CondicionVentaOtros { get; set; }

    [XmlElement("PlazoCredito")]
    public string? PlazoCredito { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: MedioPago ya no existe como elemento directo en esta posición
    // Se comenta para referencia - el medio de pago podría estar en otro lugar
    // ═══════════════════════════════════════════════════════════════
    // [XmlElement("MedioPago")]
    // public string MedioPago { get; set; } = string.Empty;

    [XmlElement("DetalleServicio")]
    public DetalleServicioXml DetalleServicio { get; set; } = new DetalleServicioXml();

    [XmlElement("OtrosCargos")]
    public OtrosCargosXml? OtrosCargos { get; set; }

    [XmlElement("ResumenFactura")]
    public ResumenFacturaXml ResumenFactura { get; set; } = new ResumenFacturaXml();

    [XmlElement("InformacionReferencia")]
    public InformacionReferenciaXml? InformacionReferencia { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: Normativa ya NO existe en el esquema
    // Fue eliminado del XSD
    // ═══════════════════════════════════════════════════════════════
    // [XmlElement("Normativa")]
    // public NormativaXml Normativa { get; set; } = new NormativaXml();

    [XmlElement("Otros")]
    public OtrosXml? Otros { get; set; }

    public FacturaElectronicaXml()
    {
        // Namespaces para v4.4
        //Xmlns.Add("", "https://tribunet.hacienda.go.cr/docs/esquemas/2017/v4.4/facturaElectronica");
        Xmlns.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
        Xmlns.Add("vc", "http://www.w3.org/2007/XMLSchema-versioning");
        Xmlns.Add("xs", "http://www.w3.org/2001/XMLSchema");
    }
}

public class EmisorXml
{
    [XmlElement("Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [XmlElement("Identificacion")]
    public IdentificacionXml Identificacion { get; set; } = new IdentificacionXml();

    [XmlElement("NombreComercial")]
    public string? NombreComercial { get; set; }

    [XmlElement("Ubicacion")]
    public UbicacionXml? Ubicacion { get; set; }

    [XmlElement("Telefono")]
    public TelefonoXml? Telefono { get; set; }

    [XmlElement("Fax")]
    public TelefonoXml? Fax { get; set; }

    [XmlElement("CorreoElectronico")]
    public string? CorreoElectronico { get; set; }
}

public class ReceptorXml
{
    [XmlElement("Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [XmlElement("Identificacion")]
    public IdentificacionXml Identificacion { get; set; } = new IdentificacionXml();

    [XmlElement("IdentificacionExtranjero")]
    public string? IdentificacionExtranjero { get; set; }

    [XmlElement("NombreComercial")]
    public string? NombreComercial { get; set; }

    [XmlElement("Ubicacion")]
    public UbicacionXml? Ubicacion { get; set; }

    [XmlElement("Telefono")]
    public TelefonoXml? Telefono { get; set; }

    [XmlElement("Fax")]
    public TelefonoXml? Fax { get; set; }

    [XmlElement("CorreoElectronico")]
    public string? CorreoElectronico { get; set; }
}

public class IdentificacionXml
{
    [XmlElement("Tipo")]
    public string Tipo { get; set; } = string.Empty;

    [XmlElement("Numero")]
    public string Numero { get; set; } = string.Empty;
}

public class UbicacionXml
{
    [XmlElement("Provincia")]
    public string Provincia { get; set; } = string.Empty;

    [XmlElement("Canton")]
    public string Canton { get; set; } = string.Empty;

    [XmlElement("Distrito")]
    public string Distrito { get; set; } = string.Empty;

    [XmlElement("Barrio")]
    public string Barrio { get; set; } = string.Empty;

    [XmlElement("OtrasSenas")]
    public string? OtrasSenas { get; set; }
}

public class TelefonoXml
{
    [XmlElement("CodigoPais")]
    public string CodigoPais { get; set; } = string.Empty;

    [XmlElement("NumTelefono")]
    public string NumTelefono { get; set; } = string.Empty;
}

public class DetalleServicioXml
{
    [XmlElement("LineaDetalle")]
    public List<LineaDetalleXml> LineasDetalle { get; set; } = new List<LineaDetalleXml>();
}

public class LineaDetalleXml
{
    // ═══════════════════════════════════════════════════════════════
    // Orden de elementos según XSD v4.4:
    // 1. NumeroLinea, 2. CodigoCABYS, 3. CodigoComercial, 4. Cantidad,
    // 5. UnidadMedida, 6. TipoTransaccion, 7. UnidadMedidaComercial,
    // 8. Detalle, 9-12. (elementos opcionales), 13. PrecioUnitario,
    // 14. MontoTotal, 15. Descuento, 16. SubTotal, 17. IVACobradoFabrica,
    // 18. BaseImponible, 19. Impuesto, 20. ImpuestoAsumidoEmisorFabrica,
    // 21. ImpuestoNeto, 22. MontoTotalLinea
    // ═══════════════════════════════════════════════════════════════

    [XmlElement("NumeroLinea")]
    public int NumeroLinea { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: PartidaArancelaria ya no existe en el esquema
    // ═══════════════════════════════════════════════════════════════
    // [XmlElement("PartidaArancelaria")]
    // public string? PartidaArancelaria { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: "Codigo" cambió a "CodigoCABYS" (Catálogo de Bienes y Servicios)
    // Es un código de 13 dígitos, ya no es un objeto con Tipo y Codigo
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("CodigoCABYS")]
    public string? CodigoCABYS { get; set; }

    [XmlElement("Cantidad")]
    public decimal Cantidad { get; set; }

    [XmlElement("UnidadMedida")]
    public string UnidadMedida { get; set; } = string.Empty;

    [XmlElement("UnidadMedidaComercial")]
    public string? UnidadMedidaComercial { get; set; }

    [XmlElement("Detalle")]
    public string Detalle { get; set; } = string.Empty;

    [XmlElement("PrecioUnitario")]
    public decimal PrecioUnitario { get; set; }

    [XmlElement("MontoTotal")]
    public decimal MontoTotal { get; set; }

    [XmlElement("Descuento")]
    public List<DescuentoXml>? Descuentos { get; set; }

    [XmlElement("SubTotal")]
    public decimal SubTotal { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: IVACobradoFabrica es opcional, va después de SubTotal
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("IVACobradoFabrica")]
    public decimal? IVACobradoFabrica { get; set; }
    public bool ShouldSerializeIVACobradoFabrica() => IVACobradoFabrica.HasValue;

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: BaseImponible es requerido cuando hay impuestos
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("BaseImponible")]
    public decimal? BaseImponible { get; set; }
    public bool ShouldSerializeBaseImponible() => BaseImponible.HasValue;

    [XmlElement("Impuesto")]
    public List<ImpuestoXml>? Impuestos { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: ImpuestoAsumidoEmisorFabrica es OBLIGATORIO según XSD
    // Se usa cuando el emisor asume impuestos o se cobran a nivel de fábrica
    // Si no aplica, enviar 0
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("ImpuestoAsumidoEmisorFabrica")]
    public decimal ImpuestoAsumidoEmisorFabrica { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: ImpuestoNeto es OBLIGATORIO según XSD
    // Se obtiene del monto del impuesto menos el monto exonerado
    // o menos ImpuestoAsumidoEmisorFabrica cuando corresponda
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("ImpuestoNeto")]
    public decimal ImpuestoNeto { get; set; }

    [XmlElement("MontoTotalLinea")]
    public decimal MontoTotalLinea { get; set; }
}

public class CodigoXml
{
    [XmlElement("Tipo")]
    public string Tipo { get; set; } = string.Empty;

    [XmlElement("Codigo")]
    public string Codigo { get; set; } = string.Empty;
}

public class DescuentoXml
{
    // ═══════════════════════════════════════════════════════════════
    // En v4.4: Orden correcto es:
    // 1. MontoDescuento
    // 2. CodigoDescuento
    // 3. NaturalezaDescuento
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("MontoDescuento")]
    public decimal MontoDescuento { get; set; }

    [XmlElement("CodigoDescuento")]
    public string CodigoDescuento { get; set; } = string.Empty;

    [XmlElement("NaturalezaDescuento")]
    public string NaturalezaDescuento { get; set; } = string.Empty;
}

public class ImpuestoXml
{
    // ═══════════════════════════════════════════════════════════════
    // En v4.4: El elemento sigue siendo "Codigo" (no "CodigoImpuesto")
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("Codigo")]
    public string Codigo { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: CodigoTarifa cambió a CodigoTarifaIVA
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("CodigoTarifaIVA")]
    public string? CodigoTarifaIVA { get; set; }

    [XmlElement("Tarifa")]
    public decimal Tarifa { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: FactorIVA cambió a FactorCalculoIVA
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("FactorCalculoIVA")]
    public decimal? FactorCalculoIVA { get; set; }
    public bool ShouldSerializeFactorCalculoIVA() => FactorCalculoIVA.HasValue;

    [XmlElement("Monto")]
    public decimal Monto { get; set; }

    [XmlElement("Exoneracion")]
    public ExoneracionXml? Exoneracion { get; set; }
}

public class ExoneracionXml
{
    [XmlElement("TipoDocumento")]
    public string TipoDocumento { get; set; } = string.Empty;

    [XmlElement("NumeroDocumento")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [XmlElement("NombreInstitucion")]
    public string NombreInstitucion { get; set; } = string.Empty;

    [XmlElement("FechaEmision")]
    public string FechaEmision { get; set; } = string.Empty;

    [XmlElement("PorcentajeExoneracion")]
    public decimal PorcentajeExoneracion { get; set; }

    [XmlElement("MontoExoneracion")]
    public decimal MontoExoneracion { get; set; }
}

public class OtrosCargosXml
{
    [XmlElement("TipoDocumento")]
    public string? TipoDocumento { get; set; }

    [XmlElement("NumeroIdentidadTercero")]
    public string? NumeroIdentidadTercero { get; set; }

    [XmlElement("NombreTercero")]
    public string? NombreTercero { get; set; }

    [XmlElement("Detalle")]
    public string? Detalle { get; set; }

    [XmlElement("Porcentaje")]
    public decimal? Porcentaje { get; set; }

    [XmlElement("MontoCargo")]
    public decimal? MontoCargo { get; set; }
}

public class ResumenFacturaXml
{
    // ═══════════════════════════════════════════════════════════════
    // En v4.4: CodigoTipoMoneda es OBLIGATORIO (no tiene minOccurs="0")
    // Siempre debe incluirse, incluso para colones (CRC)
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("CodigoTipoMoneda")]
    public CodigoTipoMonedaXml CodigoTipoMoneda { get; set; } = new CodigoTipoMonedaXml();

    [XmlElement("TotalServGravados")]
    public decimal TotalServGravados { get; set; }

    [XmlElement("TotalServExentos")]
    public decimal TotalServExentos { get; set; }

    [XmlElement("TotalServExonerado")]
    public decimal? TotalServExonerado { get; set; }
    public bool ShouldSerializeTotalServExonerado() => TotalServExonerado.HasValue;

    [XmlElement("TotalMercanciasGravadas")]
    public decimal TotalMercanciasGravadas { get; set; }

    [XmlElement("TotalMercanciasExentas")]
    public decimal TotalMercanciasExentas { get; set; }

    [XmlElement("TotalMercExonerada")]
    public decimal? TotalMercExonerada { get; set; }
    public bool ShouldSerializeTotalMercExonerada() => TotalMercExonerada.HasValue;

    [XmlElement("TotalGravado")]
    public decimal TotalGravado { get; set; }

    [XmlElement("TotalExento")]
    public decimal TotalExento { get; set; }

    [XmlElement("TotalExonerado")]
    public decimal? TotalExonerado { get; set; }
    public bool ShouldSerializeTotalExonerado() => TotalExonerado.HasValue;

    [XmlElement("TotalVenta")]
    public decimal TotalVenta { get; set; }

    [XmlElement("TotalDescuentos")]
    public decimal TotalDescuentos { get; set; }

    [XmlElement("TotalVentaNeta")]
    public decimal TotalVentaNeta { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: TotalDesgloseImpuesto es obligatorio cuando hay impuestos
    // Contiene el desglose por tipo de impuesto y tarifa
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("TotalDesgloseImpuesto")]
    public List<TotalDesgloseImpuestoXml>? TotalDesgloseImpuesto { get; set; }

    [XmlElement("TotalImpuesto")]
    public decimal TotalImpuesto { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: TotalImpAsumEmisorFabrica - total de impuestos asumidos
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("TotalImpAsumEmisorFabrica")]
    public decimal? TotalImpAsumEmisorFabrica { get; set; }
    public bool ShouldSerializeTotalImpAsumEmisorFabrica() => TotalImpAsumEmisorFabrica.HasValue;

    [XmlElement("TotalIVADevuelto")]
    public decimal? TotalIVADevuelto { get; set; }
    public bool ShouldSerializeTotalIVADevuelto() => TotalIVADevuelto.HasValue;

    [XmlElement("TotalOtrosCargos")]
    public decimal? TotalOtrosCargos { get; set; }
    public bool ShouldSerializeTotalOtrosCargos() => TotalOtrosCargos.HasValue;

    // ═══════════════════════════════════════════════════════════════
    // En v4.4: MedioPago es obligatorio excepto para créditos (02, 08, 10)
    // ═══════════════════════════════════════════════════════════════
    [XmlElement("MedioPago")]
    public List<MedioPagoXml>? MedioPago { get; set; }

    [XmlElement("TotalComprobante")]
    public decimal TotalComprobante { get; set; }
}

// ═══════════════════════════════════════════════════════════════
// Nuevo en v4.4: TotalDesgloseImpuesto
// ═══════════════════════════════════════════════════════════════
public class TotalDesgloseImpuestoXml
{
    [XmlElement("Codigo")]
    public string Codigo { get; set; } = string.Empty;

    [XmlElement("CodigoTarifaIVA")]
    public string? CodigoTarifaIVA { get; set; }

    [XmlElement("TotalMontoImpuesto")]
    public decimal TotalMontoImpuesto { get; set; }
}

// ═══════════════════════════════════════════════════════════════
// Nuevo en v4.4: MedioPago en ResumenFactura
// ═══════════════════════════════════════════════════════════════
public class MedioPagoXml
{
    [XmlElement("TipoMedioPago")]
    public string? TipoMedioPago { get; set; }

    [XmlElement("MedioPagoOtros")]
    public string? MedioPagoOtros { get; set; }

    [XmlElement("TotalMedioPago")]
    public decimal? TotalMedioPago { get; set; }
    public bool ShouldSerializeTotalMedioPago() => TotalMedioPago.HasValue;
}

public class CodigoTipoMonedaXml
{
    [XmlElement("CodigoMoneda")]
    public string CodigoMoneda { get; set; } = string.Empty;

    [XmlElement("TipoCambio")]
    public decimal TipoCambio { get; set; }
}

public class InformacionReferenciaXml
{
    [XmlElement("TipoDoc")]
    public string TipoDoc { get; set; } = string.Empty;

    [XmlElement("Numero")]
    public string Numero { get; set; } = string.Empty;

    [XmlElement("FechaEmision")]
    public string FechaEmision { get; set; } = string.Empty;

    [XmlElement("Codigo")]
    public string Codigo { get; set; } = string.Empty;

    [XmlElement("Razon")]
    public string Razon { get; set; } = string.Empty;
}

/// <summary>
/// InformacionReferencia para Notas de Crédito y Débito.
/// Hacienda v4.4 XSD: TipoDocIR, Numero, FechaEmisionIR, Codigo, Razon
/// Solo TipoDocIR y FechaEmisionIR llevan sufijo "IR"
/// </summary>
public class InformacionReferenciaNotaCreditoXml
{
    [XmlElement("TipoDocIR")]
    public string TipoDoc { get; set; } = string.Empty;

    [XmlElement("Numero")]
    public string Numero { get; set; } = string.Empty;

    [XmlElement("FechaEmisionIR")]
    public string FechaEmision { get; set; } = string.Empty;

    [XmlElement("Codigo")]
    public string Codigo { get; set; } = string.Empty;

    [XmlElement("Razon")]
    public string Razon { get; set; } = string.Empty;
}

public class NormativaXml
{
    [XmlElement("NumeroResolucion")]
    public string NumeroResolucion { get; set; } = string.Empty;

    [XmlElement("FechaResolucion")]
    public string FechaResolucion { get; set; } = string.Empty;
}

public class OtrosXml
{
    [XmlElement("OtroTexto")]
    public List<OtroTextoXml>? OtrosTextos { get; set; }
}

public class OtroTextoXml
{
    [XmlElement("codigo")]
    public string Codigo { get; set; } = string.Empty;

    [XmlElement("contenido")]
    public string Contenido { get; set; } = string.Empty;
}