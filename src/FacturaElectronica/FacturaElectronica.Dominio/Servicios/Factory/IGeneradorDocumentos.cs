using System.Xml;
using FacturaElectronica.Dominio.Modelos;

namespace FacturaElectronica.Dominio.Servicios.Factory;

public interface IGeneradorDocumentos
{
    XmlDocument CreaXMLFacturaElectronica(Factura factura, string clave, string consecutivo);
    string XmlDocumentToString(XmlDocument xmlDoc);
}