using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Modelos.Xml;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Factory;

/// <summary>
/// Generador de documentos XML para Tiquete Electrónico v4.4
/// Estructura similar a Factura pero con namespace y elemento raíz diferentes.
/// El receptor es opcional en tiquetes (típicamente B2C).
/// </summary>
public class GeneradorTiqueteV44 : IGeneradorDocumentos
{
    private readonly ILogger<GeneradorTiqueteV44> _logger;
    private readonly XmlSerializer _xmlSerializer;

    public GeneradorTiqueteV44(ILogger<GeneradorTiqueteV44> logger)
    {
        _logger = logger;
        _xmlSerializer = new XmlSerializer(typeof(TiqueteElectronicoXml));
    }

    public XmlDocument CreaXMLFacturaElectronica(Factura factura, string clave, string consecutivo)
    {
        // Este método genera XML de Tiquete aunque se llame "FacturaElectronica"
        // para mantener compatibilidad con la interfaz
        return CreaXMLTiqueteElectronico(factura, clave, consecutivo);
    }

    public XmlDocument CreaXMLTiqueteElectronico(Factura factura, string clave, string consecutivo)
    {
        try
        {
            _logger.LogInformation("Iniciando generación de XML para tiquete electrónico {Clave}", clave);

            // Mapear del modelo de dominio al modelo XML de tiquete
            var tiqueteXml = MapearFacturaATiqueteXml(factura, clave, consecutivo);

            // Validar el modelo antes de serializar
            ValidarTiqueteXml(tiqueteXml);

            // Serializar a XML
            var xmlDocument = SerializarAXml(tiqueteXml);

            _logger.LogInformation("XML de tiquete generado exitosamente para clave {Clave}", clave);
            return xmlDocument;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar XML de tiquete para clave {Clave}", clave);
            throw;
        }
    }

    private TiqueteElectronicoXml MapearFacturaATiqueteXml(Factura factura, string clave, string consecutivo)
    {
        var tiqueteXml = new TiqueteElectronicoXml
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

            CondicionVenta = factura.CondicionVenta ?? "01", // Tiquetes típicamente son contado
            PlazoCredito = factura.CondicionVenta == "02" ? factura.PlazoCredito.ToString() : null,

            DetalleServicio = MapearDetalleServicio(factura),
            ResumenFactura = MapearResumenFactura(factura)
        };

        // En tiquetes, el receptor es OPCIONAL
        // Solo se incluye si explícitamente se indica y tiene datos válidos
        if (factura.Receptor && !string.IsNullOrEmpty(factura.ReceptorNumeroIdentificacion))
        {
            _logger.LogInformation("Tiquete incluye receptor: {ReceptorId}", factura.ReceptorNumeroIdentificacion);
            tiqueteXml.Receptor = MapearReceptor(factura);
        }
        else
        {
            _logger.LogInformation("Tiquete sin receptor (venta B2C)");
        }

        return tiqueteXml;
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
                        CodigoDescuento = "07", // Descuento Comercial
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

        // MedioPago es obligatorio para tiquetes (típicamente contado)
        if (factura.CondicionVenta != "02" && factura.CondicionVenta != "08" && factura.CondicionVenta != "10")
        {
            resumen.MedioPago = new List<MedioPagoXml>
            {
                new MedioPagoXml
                {
                    TipoMedioPago = factura.MedioPago ?? "01" // Default: Efectivo
                }
            };
        }

        return resumen;
    }

    private void ValidarTiqueteXml(TiqueteElectronicoXml tiqueteXml)
    {
        var errores = new List<string>();

        if (string.IsNullOrEmpty(tiqueteXml.Clave))
            errores.Add("La clave es requerida");

        if (string.IsNullOrEmpty(tiqueteXml.NumeroConsecutivo))
            errores.Add("El número consecutivo es requerido");

        if (string.IsNullOrEmpty(tiqueteXml.Emisor.Nombre))
            errores.Add("El nombre del emisor es requerido");

        if (string.IsNullOrEmpty(tiqueteXml.Emisor.Identificacion.Numero))
            errores.Add("La identificación del emisor es requerida");

        if (tiqueteXml.DetalleServicio.LineasDetalle.Count == 0)
            errores.Add("Debe incluir al menos una línea de detalle");

        if (string.IsNullOrEmpty(tiqueteXml.CodigoActividad))
            errores.Add("El código de actividad CIIU es requerido en v4.4");

        if (errores.Any())
        {
            var mensaje = $"Errores de validación de tiquete: {string.Join(", ", errores)}";
            _logger.LogError(mensaje);
            throw new ArgumentException(mensaje);
        }
    }

    private XmlDocument SerializarAXml(TiqueteElectronicoXml tiqueteXml)
    {
        using var memoryStream = new MemoryStream();
        using var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = false,
            OmitXmlDeclaration = false
        });

        _xmlSerializer.Serialize(xmlWriter, tiqueteXml);

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
