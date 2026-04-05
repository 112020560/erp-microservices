using FacturaElectronica.Dominio.Modelos;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.CodigosHacienda;

// <summary>
/// Interfaz para validar códigos según las tablas de referencia de Hacienda
/// </summary>
public interface IValidadorCodigosHacienda
{
    ResultadoValidacion ValidarFactura(Factura factura);
    ResultadoValidacion ValidarTipoIdentificacion(string tipoIdentificacion);
    ResultadoValidacion ValidarCodigoActividad(string codigoActividad);
    ResultadoValidacion ValidarCondicionVenta(string condicionVenta);
    ResultadoValidacion ValidarMedioPago(string medioPago);
    ResultadoValidacion ValidarUnidadMedida(string unidadMedida);
    ResultadoValidacion ValidarCodigoCABYS(string codigoCABYS);
    ResultadoValidacion ValidarCodigoImpuesto(string codigoImpuesto);
    ResultadoValidacion ValidarCodigoMoneda(string codigoMoneda);
    ResultadoValidacion ValidarCodigoPais(string codigoPais);
}