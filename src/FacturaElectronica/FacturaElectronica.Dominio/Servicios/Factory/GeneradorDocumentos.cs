using System.Text;
using System.Xml;
using FacturaElectronica.Dominio.Modelos;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Factory;

public class GeneradorDocumentos : IGeneradorDocumentos
{

	private readonly ILogger<GeneradorDocumentos> _logger;
	public GeneradorDocumentos(ILogger<GeneradorDocumentos> logger)
	{
		_logger = logger;
	}
	public XmlDocument CreaXMLFacturaElectronica(Factura factura, string clave, string consecutivo)
	{
		try
		{
			var mXML = new MemoryStream();

			XmlTextWriter writer = new XmlTextWriter(mXML, System.Text.Encoding.UTF8);

			XmlDocument docXML = new XmlDocument();

			GeneraXML(writer, factura, clave, consecutivo);

			mXML.Seek(0, SeekOrigin.Begin);

			docXML.Load(mXML);

			writer.Close();

			// Retorna el documento xml y ahi se puede salvar docXML.Save
			return docXML;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al crear XML de factura electrónica");
			throw;
		}
	}



	private void GeneraXML(XmlTextWriter writer, Factura factura, string clave, string consecutivo) // As System.Xml.XmlTextWriter
	{
		try
		{
			writer.WriteStartDocument();
			writer.WriteStartElement("FacturaElectronica");

			writer.WriteAttributeString("xmlns", "https://tribunet.hacienda.go.cr/docs/esquemas/2017/v4.2/facturaElectronica");
			writer.WriteAttributeString("xmlns:ds", "http://www.w3.org/2000/09/xmldsig#");
			writer.WriteAttributeString("xmlns:vc", "http://www.w3.org/2007/XMLSchema-versioning");
			writer.WriteAttributeString("xmlns:xs", "http://www.w3.org/2001/XMLSchema");

			// La clave se crea con la función CreaClave de la clase Datos
			writer.WriteElementString("Clave", clave);

			// 'El numero de secuencia es de 20 caracteres, 
			// 'Se debe de crear con la función CreaNumeroSecuencia de la clase Datos
			writer.WriteElementString("NumeroConsecutivo", consecutivo);

			// 'El formato de la fecha es yyyy-MM-ddTHH:mm:sszzz
			writer.WriteElementString("FechaEmision", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"));

			writer.WriteStartElement("Emisor");

			writer.WriteElementString("Nombre", factura.EmisorNombre);
			writer.WriteStartElement("Identificacion");
			writer.WriteElementString("Tipo", factura.EmisorTipoIdentificacion);
			writer.WriteElementString("Numero", factura.EmisorNumeroIdentificacion);
			writer.WriteEndElement(); // 'Identificacion

			// '-----------------------------------
			// 'Los datos de las ubicaciones los puede tomar de las tablas de datos, 
			// 'Debe ser exacto al que hacienda tiene registrado para su compañia
			if (!string.IsNullOrEmpty(factura.EmisorProvincia) &&
				!string.IsNullOrEmpty(factura.EmisorCanton) &&
				!string.IsNullOrEmpty(factura.EmisorDistrito) &&
				!string.IsNullOrEmpty(factura.EmisorBarrio) &&
				!string.IsNullOrEmpty(factura.EmisorOtrasSenas))
			{
				writer.WriteStartElement("Ubicacion");
				writer.WriteElementString("Provincia", factura.EmisorProvincia);
				writer.WriteElementString("Canton", factura.EmisorCanton);
				writer.WriteElementString("Distrito", factura.EmisorDistrito);
				writer.WriteElementString("Barrio", factura.EmisorBarrio);
				writer.WriteElementString("OtrasSenas", factura.EmisorOtrasSenas);
				writer.WriteEndElement(); // 'Ubicacion
			}

			if (!string.IsNullOrEmpty(factura.EmisorCodigoPaisTelefono) &&
				!string.IsNullOrEmpty(factura.EmisorTelefono))
			{
				writer.WriteStartElement("Telefono");
				writer.WriteElementString("CodigoPais", factura.EmisorCodigoPaisTelefono);
				writer.WriteElementString("NumTelefono", factura.EmisorTelefono);
				writer.WriteEndElement(); // 'Telefono
			}

			if (!string.IsNullOrEmpty(factura.EmisorCorreoElectronico))
			{
				writer.WriteElementString("CorreoElectronico", factura.EmisorCorreoElectronico);
			}
			writer.WriteEndElement(); // Emisor
									  // '------------------------------------
									  // 'Receptor es similar con emisor, los datos opcionales siempre siempre siempre omitirlos.
									  // 'La ubicacion para el receptor es opcional por ejemplo
			if (factura.Receptor)
			{
				writer.WriteStartElement("Receptor");
				writer.WriteElementString("Nombre", factura.ReceptorNombre);
				writer.WriteStartElement("Identificacion");
				// 'Los tipos de identificacion los puede ver en la tabla de datos
				writer.WriteElementString("Tipo", factura.ReceptorTipoIdentificacion);
				writer.WriteElementString("Numero", factura.ReceptorNumeroIdentificacion);
				writer.WriteEndElement(); // 'Identificacion

				writer.WriteStartElement("Telefono");
				writer.WriteElementString("CodigoPais", factura.ReceptorCodigoPais);
				writer.WriteElementString("NumTelefono", factura.ReceptorTelefono);
				writer.WriteEndElement(); // 'Telefono

				writer.WriteElementString("CorreoElectronico", factura.ReceptorCorreoElectronico);

				writer.WriteEndElement(); // Receptor
			}
			// '------------------------------------

			// 'Loa datos estan en la tabla correspondiente
			writer.WriteElementString("CondicionVenta", factura.CondicionVenta);
			// '01: Contado
			// '02: Credito
			// '03: Consignación
			// '04: Apartado
			// '05: Arrendamiento con opcion de compra
			// '06: Arrendamiento con función financiera
			// '99: Otros
			if (factura.CondicionVenta.Equals("02"))
			{
				// 'Este dato se muestra si la condicion venta es credito
				writer.WriteElementString("PlazoCredito", factura.PlazoCredito.ToString());
			}

			writer.WriteElementString("MedioPago", factura.MedioPago);
			// '01: Efectivo
			// '02: Tarjeta
			// '03: Cheque
			// '04: Transferecia - deposito bancario
			// '05: Recaudado por terceros            
			// '99: Otros

			writer.WriteStartElement("DetalleServicio");

			// '-------------------------------------
			foreach (var dr in factura.DetalleServicios ?? [])
			{
				writer.WriteStartElement("LineaDetalle");

				writer.WriteElementString("NumeropLinea", dr.NumeroLinea.ToString());

				writer.WriteStartElement("Codigo");
				writer.WriteElementString("Tipo", dr.ArticuloTipo);
				writer.WriteElementString("Codigo", dr.CodigoArticulo);
				writer.WriteEndElement(); // 'Codigo

				writer.WriteElementString("Cantidad", dr.Cantidad.ToString());
				// 'Para las unidades de medida ver la tabla correspondiente
				writer.WriteElementString("UnidadMedida", dr.UnidadMedida);
				writer.WriteElementString("Detalle", dr.DetalleArticulo);
				writer.WriteElementString("PrecioUnitario", string.Format("{0:N3}", dr.PrecioUnitario.ToString()));
				writer.WriteElementString("MontoTotal", string.Format("{0:N3}", dr.Precio.ToString()));
				writer.WriteElementString("MontoDescuento", string.Format("{0:N3}", dr.Descuento.ToString()));
				writer.WriteElementString("NaturalezaDescuento", dr.NaturalezaDescuento.ToString());
				writer.WriteElementString("SubTotal", string.Format("{0:N3}", dr.SubTotal.ToString()));

				writer.WriteStartElement("Impuesto");
				writer.WriteElementString("Codigo", dr.CodigoImpuesto);
				writer.WriteElementString("Tarifa", dr.TarifaImpuesto.ToString());
				writer.WriteElementString("Monto", dr.MontoImpuesto.ToString());
				writer.WriteEndElement(); // Impuesto

				writer.WriteElementString("MontoTotalLinea", string.Format("{0:N3}", dr.MontoTotalLinea.ToString()));

				writer.WriteEndElement(); // LineaDetalle
			}
			// '-------------------------------------

			writer.WriteEndElement(); // DetalleServicio


			writer.WriteStartElement("ResumenFactura");

			// Estos campos son opcionales, solo fin desea facturar en dólares
			writer.WriteElementString("CodigoMoneda", factura.CodigoMoneda);
			writer.WriteElementString("TipoCambio", factura.TipoCambio);
			// =================

			// 'En esta parte los totales se pueden ir sumando linea a linea cuando se carga el detalle
			// 'ó se pasa como parametros al inicio
			writer.WriteElementString("TotalServGravados", factura.TotalServGravados.ToString("N3"));
			writer.WriteElementString("TotalServExentos", factura.TotalServExentos.ToString("N3"));
			writer.WriteElementString("TotalMercanciasGravadas", factura.TotalMercanciasGravadas.ToString("N3"));
			writer.WriteElementString("TotalMercanciasExentas", factura.TotalMercanciasExentas.ToString("N3"));

			writer.WriteElementString("TotalGravado", factura.TotalGravado.ToString("N3"));
			writer.WriteElementString("TotalExento", factura.TotalExento.ToString("N3"));

			writer.WriteElementString("TotalVenta", factura.TotalVenta.ToString("N3"));
			writer.WriteElementString("TotalDescuentos", factura.TotalDescuentos.ToString("N3"));
			writer.WriteElementString("TotalVentaNeta", factura.TotalVentaNeta.ToString("N3"));
			writer.WriteElementString("TotalImpuesto", factura.TotalImpuesto.ToString("N3"));
			writer.WriteElementString("TotalComprobante", factura.TotalComprobante.ToString("N3"));
			writer.WriteEndElement(); // ResumenFactura

			// 'Estos datos te los tiene que brindar los encargados del area financiera
			writer.WriteStartElement("Normativa");
			writer.WriteElementString("NumeroResolucion", factura.NumeroResolucion);
			writer.WriteElementString("FechaResolucion", factura.FechaResolucion);
			writer.WriteEndElement(); // Normativa

			// 'Aqui va la firma, despues la agregamos.

			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al generar XML de factura electrónica");
			throw;
		}
	}

	public string XmlDocumentToString(XmlDocument xmlDoc)
	{
		using var stringWriter = new StringWriter();
		using var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
		{
			Encoding = Encoding.UTF8,
			OmitXmlDeclaration = false,   // en Hacienda sí se requiere la declaración XML
			Indent = false                // sin identar (evita espacios innecesarios)
		});

		xmlDoc.WriteTo(xmlTextWriter);
		xmlTextWriter.Flush();

		return stringWriter.ToString();
	}
}