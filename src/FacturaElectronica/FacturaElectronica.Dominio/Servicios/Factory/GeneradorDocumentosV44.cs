using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Modelos.Xml;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Factory;

public class GeneradorDocumentosV44 : IGeneradorDocumentos
{
    private readonly ILogger<GeneradorDocumentosV44> _logger;
    private readonly XmlSerializer _xmlSerializer;

    public GeneradorDocumentosV44(ILogger<GeneradorDocumentosV44> logger)
    {
        _logger = logger;
        //:TODO Validar formato de la versión 4.4
        _xmlSerializer = new XmlSerializer(typeof(FacturaElectronicaXml));
    }

    public XmlDocument CreaXMLFacturaElectronica(Factura factura, string clave, string consecutivo)
    {
        try
        {
            _logger.LogInformation("Iniciando generación de XML para factura {Clave}", clave);

            // Mapear del modelo de dominio al modelo XML
            var facturaXml = MapearFacturaAXml(factura, clave, consecutivo);

            // Validar el modelo antes de serializar
            ValidarFacturaXml(facturaXml);

            // Serializar a XML
            var xmlDocument = SerializarAXml(facturaXml);

            _logger.LogInformation("XML generado exitosamente para factura {Clave}", clave);
            return xmlDocument;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar XML para factura {Clave}", clave);
            throw;
        }
    }

    private FacturaElectronicaXml MapearFacturaAXml(Factura factura, string clave, string consecutivo)
    {
        var facturaXml = new FacturaElectronicaXml
        {
            Clave = clave,
            // ═══════════════════════════════════════════════════════════════
            // NUEVO en v4.4: ProveedorSistemas es requerido
            // Debe ir después de Clave y antes de CodigoActividad
            // Si no se envía, usa la cédula del emisor como fallback
            // ═══════════════════════════════════════════════════════════════
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

            CondicionVenta = factura.CondicionVenta ?? string.Empty,
            PlazoCredito = factura.CondicionVenta == "02" ? factura.PlazoCredito.ToString() : null,
            // MedioPago ya no existe en v4.4 como elemento directo
            // MedioPago = factura.MedioPago ?? string.Empty,

            DetalleServicio = MapearDetalleServicio(factura),
            ResumenFactura = MapearResumenFactura(factura)

            // ═══════════════════════════════════════════════════════════════
            // En v4.4: Normativa ya NO existe en el esquema - fue eliminado
            // ═══════════════════════════════════════════════════════════════
            // Normativa = new NormativaXml
            // {
            //     NumeroResolucion = factura.NumeroResolucion ?? string.Empty,
            //     FechaResolucion = factura.FechaResolucion ?? string.Empty
            // }
        };

        // Mapear receptor solo si existe
        if (factura.Receptor && !string.IsNullOrEmpty(factura.ReceptorNumeroIdentificacion))
        {
            facturaXml.Receptor = MapearReceptor(factura);
        }

        return facturaXml;
    }

    private UbicacionXml? MapearUbicacionEmisor(Factura factura)
    {
        if (string.IsNullOrEmpty(factura.EmisorProvincia) ||
            string.IsNullOrEmpty(factura.EmisorCanton) ||
            string.IsNullOrEmpty(factura.EmisorDistrito) ||
            string.IsNullOrEmpty(factura.EmisorBarrio))
            return null;

        // ═══════════════════════════════════════════════════════════════
        // En v4.4 el código de Barrio debe tener mínimo 5 caracteres
        // Se forma concatenando: Provincia(1) + Canton(2) + Barrio(2) = 5 dígitos
        // Ejemplo: Provincia="1", Canton="01", Barrio="01" → "10101"
        // ═══════════════════════════════════════════════════════════════
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

    /// <summary>
    /// Formatea el código de barrio para cumplir con el mínimo de 5 caracteres de v4.4
    /// Concatena: Provincia(1) + Canton(2) + Barrio(2) = 5 dígitos
    /// </summary>
    private string FormatearCodigoBarrio(string provincia, string canton, string barrio)
    {
        // Asegurar que cada parte tenga el tamaño correcto
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
                // ═══════════════════════════════════════════════════════════════
                // En v4.4: CodigoCABYS reemplaza a Codigo (es un string de 13 dígitos)
                // ═══════════════════════════════════════════════════════════════
                // Codigo = new CodigoXml
                // {
                //     Tipo = detalle.ArticuloTipo ?? string.Empty,
                //     Codigo = detalle.CodigoArticulo ?? string.Empty
                // },
                CodigoCABYS = detalle.CodigoArticulo ?? string.Empty,
                Cantidad = detalle.Cantidad,
                UnidadMedida = detalle.UnidadMedida ?? string.Empty,
                Detalle = detalle.DetalleArticulo ?? string.Empty,
                PrecioUnitario = detalle.PrecioUnitario,
                MontoTotal = detalle.Precio,
                SubTotal = detalle.SubTotal,

                // Mapear descuentos si existen
                // ═══════════════════════════════════════════════════════════════
                // Códigos de Descuento v4.4:
                // 01=Regalía (100%), 02=Regalía IVA, 03=Bonificación, 04=Volumen
                // 05=Temporada, 06=Promocional, 07=Comercial, 08=Frecuencia
                // 09=Sostenido, 99=Otros
                // ═══════════════════════════════════════════════════════════════
                Descuentos = detalle.Descuento > 0 ? new List<DescuentoXml>
                {
                    new DescuentoXml
                    {
                        MontoDescuento = detalle.Descuento,
                        // CodigoDescuento = "01", // ❌ 01=Regalía requiere 100% descuento
                        CodigoDescuento = "07", // ✅ 07=Descuento Comercial
                        NaturalezaDescuento = detalle.NaturalezaDescuento ?? "Descuento comercial"
                    }
                } : null,

                // ═══════════════════════════════════════════════════════════════
                // En v4.4: BaseImponible es requerido antes de Impuesto
                // Es la base sobre la cual se calcula el impuesto (SubTotal)
                // ═══════════════════════════════════════════════════════════════
                BaseImponible = detalle.SubTotal,

                // Mapear impuestos
                // En v4.4: Codigo y CodigoTarifaIVA son requeridos
                Impuestos = new List<ImpuestoXml>
                {
                    new ImpuestoXml
                    {
                        Codigo = detalle.CodigoImpuesto ?? "01", // 01 = IVA
                        CodigoTarifaIVA = "08", // 08 = Tarifa general 13%
                        Tarifa = detalle.TarifaImpuesto,
                        Monto = detalle.MontoImpuesto
                    }
                },

                // ═══════════════════════════════════════════════════════════════
                // En v4.4: ImpuestoAsumidoEmisorFabrica es OBLIGATORIO
                // Si el emisor no asume impuestos, enviar 0
                // ═══════════════════════════════════════════════════════════════
                ImpuestoAsumidoEmisorFabrica = 0,

                // ═══════════════════════════════════════════════════════════════
                // En v4.4: ImpuestoNeto es OBLIGATORIO
                // MontoImpuesto - MontoExonerado - ImpuestoAsumidoEmisorFabrica
                // ═══════════════════════════════════════════════════════════════
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
            // ═══════════════════════════════════════════════════════════════
            // En v4.4: CodigoTipoMoneda es OBLIGATORIO siempre
            // Incluso para colones (CRC) debe incluirse
            // ═══════════════════════════════════════════════════════════════
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

            // ═══════════════════════════════════════════════════════════════
            // En v4.4: TotalDesgloseImpuesto es obligatorio cuando hay impuestos
            // Agrupa los impuestos por código y tarifa
            // ═══════════════════════════════════════════════════════════════
            TotalDesgloseImpuesto = factura.TotalImpuesto > 0 ? new List<TotalDesgloseImpuestoXml>
            {
                new TotalDesgloseImpuestoXml
                {
                    Codigo = "01", // IVA
                    CodigoTarifaIVA = "08", // Tarifa general 13%
                    TotalMontoImpuesto = factura.TotalImpuesto
                }
            } : null,

            TotalImpuesto = factura.TotalImpuesto,
            TotalComprobante = factura.TotalComprobante
        };

        // ═══════════════════════════════════════════════════════════════
        // En v4.4: MedioPago es obligatorio excepto para créditos (02, 08, 10)
        // ═══════════════════════════════════════════════════════════════
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

    private void ValidarFacturaXml(FacturaElectronicaXml facturaXml)
    {
        var errores = new List<string>();

        if (string.IsNullOrEmpty(facturaXml.Clave))
            errores.Add("La clave es requerida");

        if (string.IsNullOrEmpty(facturaXml.NumeroConsecutivo))
            errores.Add("El número consecutivo es requerido");

        if (string.IsNullOrEmpty(facturaXml.Emisor.Nombre))
            errores.Add("El nombre del emisor es requerido");

        if (string.IsNullOrEmpty(facturaXml.Emisor.Identificacion.Numero))
            errores.Add("La identificación del emisor es requerida");

        if (facturaXml.DetalleServicio.LineasDetalle.Count == 0)
            errores.Add("Debe incluir al menos una línea de detalle");

        // Validaciones específicas para v4.4
        if (string.IsNullOrEmpty(facturaXml.CodigoActividad))
            errores.Add("El código de actividad CIIU es requerido en v4.4");

        if (errores.Any())
        {
            var mensaje = $"Errores de validación: {string.Join(", ", errores)}";
            _logger.LogError(mensaje);
            throw new ArgumentException(mensaje);
        }
    }

    private XmlDocument SerializarAXml(FacturaElectronicaXml facturaXml)
    {
        using var memoryStream = new MemoryStream();
        using var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = false,
            OmitXmlDeclaration = false
        });

        _xmlSerializer.Serialize(xmlWriter, facturaXml);

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