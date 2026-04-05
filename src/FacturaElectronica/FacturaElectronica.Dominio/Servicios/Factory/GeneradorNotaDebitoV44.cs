using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Modelos.Xml;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Factory;

/// <summary>
/// Generador de documentos XML para Nota de Débito Electrónica v4.4
/// InformacionReferencia es OBLIGATORIA para Notas de Débito.
/// </summary>
public class GeneradorNotaDebitoV44 : IGeneradorDocumentos
{
    private readonly ILogger<GeneradorNotaDebitoV44> _logger;
    private readonly XmlSerializer _xmlSerializer;

    public GeneradorNotaDebitoV44(ILogger<GeneradorNotaDebitoV44> logger)
    {
        _logger = logger;
        _xmlSerializer = new XmlSerializer(typeof(NotaDebitoElectronicaXml));
    }

    public XmlDocument CreaXMLFacturaElectronica(Factura factura, string clave, string consecutivo)
    {
        return CreaXMLNotaDebitoElectronica(factura, clave, consecutivo);
    }

    public XmlDocument CreaXMLNotaDebitoElectronica(Factura factura, string clave, string consecutivo)
    {
        try
        {
            _logger.LogInformation("Iniciando generación de XML para nota de débito electrónica {Clave}", clave);

            // Validar InformacionReferencia obligatoria
            ValidarInformacionReferencia(factura.InformacionReferencia);

            // Mapear del modelo de dominio al modelo XML
            var notaDebitoXml = MapearFacturaANotaDebitoXml(factura, clave, consecutivo);

            // Validar el modelo antes de serializar
            ValidarNotaDebitoXml(notaDebitoXml);

            // Serializar a XML
            var xmlDocument = SerializarAXml(notaDebitoXml);

            _logger.LogInformation("XML de nota de débito generado exitosamente para clave {Clave}", clave);
            return xmlDocument;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar XML de nota de débito para clave {Clave}", clave);
            throw;
        }
    }

    private void ValidarInformacionReferencia(InformacionReferencia? informacionReferencia)
    {
        if (informacionReferencia == null)
            throw new ArgumentException("InformacionReferencia es OBLIGATORIA para Notas de Débito");

        if (string.IsNullOrEmpty(informacionReferencia.Numero))
            throw new ArgumentException("El número del documento de referencia es requerido");

        if (informacionReferencia.Numero.Length != 50)
            throw new ArgumentException($"El número del documento de referencia debe tener 50 dígitos. Actual: {informacionReferencia.Numero.Length}");

        if (string.IsNullOrEmpty(informacionReferencia.Razon))
            throw new ArgumentException("La razón de referencia es requerida");

        if (informacionReferencia.Razon.Length > 180)
            throw new ArgumentException($"La razón de referencia no puede exceder 180 caracteres. Actual: {informacionReferencia.Razon.Length}");

        var codigosValidos = new[] { "01", "02", "03", "04", "05", "99" };
        if (!codigosValidos.Contains(informacionReferencia.Codigo))
            throw new ArgumentException($"Código de referencia inválido: {informacionReferencia.Codigo}. Válidos: {string.Join(", ", codigosValidos)}");

        var tiposDocValidos = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "99" };
        if (!tiposDocValidos.Contains(informacionReferencia.TipoDoc))
            throw new ArgumentException($"Tipo de documento de referencia inválido: {informacionReferencia.TipoDoc}");
    }

    private NotaDebitoElectronicaXml MapearFacturaANotaDebitoXml(Factura factura, string clave, string consecutivo)
    {
        var notaDebitoXml = new NotaDebitoElectronicaXml
        {
            Clave = clave,
            ProveedorSistemas = !string.IsNullOrEmpty(factura.ProveedorSistemas)
                ? factura.ProveedorSistemas
                : factura.EmisorNumeroIdentificacion ?? string.Empty,
            CodigoActividad = factura.CodigoActividad ?? string.Empty,
            NumeroConsecutivo = consecutivo,
            FechaEmision = factura.FechaDocumento.ToString("yyyy-MM-ddTHH:mm:sszzz"),

            Emisor = new EmisorXml
            {
                Nombre = factura.EmisorNombre ?? string.Empty,
                Identificacion = new IdentificacionXml
                {
                    Tipo = factura.EmisorTipoIdentificacion ?? string.Empty,
                    Numero = factura.EmisorNumeroIdentificacion ?? string.Empty
                },
                NombreComercial = factura.EmisorNombreComercial,
                Ubicacion = MapearUbicacionEmisor(factura),
                Telefono = MapearTelefonoEmisor(factura),
                CorreoElectronico = factura.EmisorCorreoElectronico
            },

            CondicionVenta = factura.CondicionVenta ?? "01",
            // ND no tiene plazo de crédito - es un documento de corrección
            PlazoCredito = null,

            DetalleServicio = MapearDetalleServicio(factura),
            ResumenFactura = MapearResumenFactura(factura),

            // InformacionReferencia OBLIGATORIA
            InformacionReferencia = MapearInformacionReferencia(factura.InformacionReferencia!)
        };

        // Mapear receptor si existe
        if (factura.Receptor && !string.IsNullOrEmpty(factura.ReceptorNumeroIdentificacion))
        {
            notaDebitoXml.Receptor = MapearReceptor(factura);
        }

        return notaDebitoXml;
    }

    private InformacionReferenciaNotaCreditoXml MapearInformacionReferencia(InformacionReferencia info)
    {
        return new InformacionReferenciaNotaCreditoXml
        {
            TipoDoc = info.TipoDoc,
            Numero = info.Numero,
            FechaEmision = info.FechaEmision.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            Codigo = info.Codigo,
            Razon = info.Razon
        };
    }

    private UbicacionXml? MapearUbicacionEmisor(Factura factura)
    {
        if (string.IsNullOrEmpty(factura.EmisorProvincia) ||
            string.IsNullOrEmpty(factura.EmisorCanton) ||
            string.IsNullOrEmpty(factura.EmisorDistrito) ||
            string.IsNullOrEmpty(factura.EmisorBarrio))
            return null;

        var codigoBarrio = FormatearCodigoBarrio(
            factura.EmisorProvincia,
            factura.EmisorCanton,
            factura.EmisorBarrio);

        return new UbicacionXml
        {
            Provincia = factura.EmisorProvincia,
            Canton = factura.EmisorCanton,
            Distrito = factura.EmisorDistrito,
            Barrio = codigoBarrio,
            OtrasSenas = factura.EmisorOtrasSenas
        };
    }

    private string FormatearCodigoBarrio(string provincia, string canton, string barrio)
    {
        var prov = provincia.PadLeft(1, '0');
        var cant = canton.PadLeft(2, '0');
        var bar = barrio.PadLeft(2, '0');
        return $"{prov}{cant}{bar}";
    }

    private TelefonoXml? MapearTelefonoEmisor(Factura factura)
    {
        if (string.IsNullOrEmpty(factura.EmisorCodigoPaisTelefono) ||
            string.IsNullOrEmpty(factura.EmisorTelefono))
            return null;

        return new TelefonoXml
        {
            CodigoPais = factura.EmisorCodigoPaisTelefono,
            NumTelefono = factura.EmisorTelefono
        };
    }

    private ReceptorXml MapearReceptor(Factura factura)
    {
        return new ReceptorXml
        {
            Nombre = factura.ReceptorNombre ?? string.Empty,
            Identificacion = new IdentificacionXml
            {
                Tipo = factura.ReceptorTipoIdentificacion ?? string.Empty,
                Numero = factura.ReceptorNumeroIdentificacion ?? string.Empty
            },
            Telefono = string.IsNullOrEmpty(factura.ReceptorCodigoPais) ||
                      string.IsNullOrEmpty(factura.ReceptorTelefono) ? null :
                new TelefonoXml
                {
                    CodigoPais = factura.ReceptorCodigoPais,
                    NumTelefono = factura.ReceptorTelefono
                },
            CorreoElectronico = factura.ReceptorCorreoElectronico
        };
    }

    private DetalleServicioXml MapearDetalleServicio(Factura factura)
    {
        var detalleServicio = new DetalleServicioXml();

        if (factura.DetalleServicios != null)
        {
            detalleServicio.LineasDetalle = factura.DetalleServicios.Select(detalle => new LineaDetalleXml
            {
                NumeroLinea = detalle.NumeroLinea,
                CodigoCABYS = detalle.CodigoArticulo ?? string.Empty,
                Cantidad = detalle.Cantidad,
                UnidadMedida = detalle.UnidadMedida ?? string.Empty,
                Detalle = detalle.DetalleArticulo ?? string.Empty,
                PrecioUnitario = detalle.PrecioUnitario,
                MontoTotal = detalle.Precio,
                SubTotal = detalle.SubTotal,

                Descuentos = detalle.Descuento > 0 ? new List<DescuentoXml>
                {
                    new DescuentoXml
                    {
                        MontoDescuento = detalle.Descuento,
                        CodigoDescuento = "07",
                        NaturalezaDescuento = detalle.NaturalezaDescuento ?? "Descuento comercial"
                    }
                } : null,

                BaseImponible = detalle.SubTotal,

                Impuestos = new List<ImpuestoXml>
                {
                    new ImpuestoXml
                    {
                        Codigo = detalle.CodigoImpuesto ?? "01",
                        CodigoTarifaIVA = "08",
                        Tarifa = detalle.TarifaImpuesto,
                        Monto = detalle.MontoImpuesto
                    }
                },

                ImpuestoAsumidoEmisorFabrica = 0,
                ImpuestoNeto = detalle.MontoImpuesto,
                MontoTotalLinea = detalle.MontoTotalLinea
            }).ToList();
        }

        return detalleServicio;
    }

    private ResumenFacturaXml MapearResumenFactura(Factura factura)
    {
        var resumen = new ResumenFacturaXml
        {
            CodigoTipoMoneda = new CodigoTipoMonedaXml
            {
                CodigoMoneda = !string.IsNullOrEmpty(factura.CodigoMoneda) ? factura.CodigoMoneda : "CRC",
                TipoCambio = decimal.Parse(factura.TipoCambio ?? "1")
            },
            TotalServGravados = factura.TotalServGravados,
            TotalServExentos = factura.TotalServExentos,
            TotalMercanciasGravadas = factura.TotalMercanciasGravadas,
            TotalMercanciasExentas = factura.TotalMercanciasExentas,
            TotalGravado = factura.TotalGravado,
            TotalExento = factura.TotalExento,
            TotalVenta = factura.TotalVenta,
            TotalDescuentos = factura.TotalDescuentos,
            TotalVentaNeta = factura.TotalVentaNeta,

            TotalDesgloseImpuesto = factura.TotalImpuesto > 0 ? new List<TotalDesgloseImpuestoXml>
            {
                new TotalDesgloseImpuestoXml
                {
                    Codigo = "01",
                    CodigoTarifaIVA = "08",
                    TotalMontoImpuesto = factura.TotalImpuesto
                }
            } : null,

            TotalImpuesto = factura.TotalImpuesto,
            TotalComprobante = factura.TotalComprobante
        };

        // MedioPago es obligatorio excepto para créditos
        if (factura.CondicionVenta != "02" && factura.CondicionVenta != "08" && factura.CondicionVenta != "10")
        {
            resumen.MedioPago = new List<MedioPagoXml>
            {
                new MedioPagoXml
                {
                    TipoMedioPago = factura.MedioPago ?? "01"
                }
            };
        }

        return resumen;
    }

    private void ValidarNotaDebitoXml(NotaDebitoElectronicaXml notaDebitoXml)
    {
        var errores = new List<string>();

        if (string.IsNullOrEmpty(notaDebitoXml.Clave))
            errores.Add("La clave es requerida");

        if (string.IsNullOrEmpty(notaDebitoXml.NumeroConsecutivo))
            errores.Add("El número consecutivo es requerido");

        if (string.IsNullOrEmpty(notaDebitoXml.Emisor.Nombre))
            errores.Add("El nombre del emisor es requerido");

        if (string.IsNullOrEmpty(notaDebitoXml.Emisor.Identificacion.Numero))
            errores.Add("La identificación del emisor es requerida");

        if (notaDebitoXml.DetalleServicio.LineasDetalle.Count == 0)
            errores.Add("Debe incluir al menos una línea de detalle");

        if (string.IsNullOrEmpty(notaDebitoXml.CodigoActividad))
            errores.Add("El código de actividad CIIU es requerido en v4.4");

        // Validar InformacionReferencia obligatoria
        if (string.IsNullOrEmpty(notaDebitoXml.InformacionReferencia.Numero))
            errores.Add("La información de referencia es obligatoria para Notas de Débito");

        if (errores.Any())
        {
            var mensaje = $"Errores de validación de nota de débito: {string.Join(", ", errores)}";
            _logger.LogError(mensaje);
            throw new ArgumentException(mensaje);
        }
    }

    private XmlDocument SerializarAXml(NotaDebitoElectronicaXml notaDebitoXml)
    {
        using var memoryStream = new MemoryStream();
        using var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = false,
            OmitXmlDeclaration = false
        });

        _xmlSerializer.Serialize(xmlWriter, notaDebitoXml);

        memoryStream.Position = 0;
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(memoryStream);

        return xmlDocument;
    }

    public string XmlDocumentToString(XmlDocument xmlDoc)
    {
        using var stringWriter = new StringWriter();
        using var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false,
            Indent = false
        });

        xmlDoc.WriteTo(xmlTextWriter);
        xmlTextWriter.Flush();

        return stringWriter.ToString();
    }
}
