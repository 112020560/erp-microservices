using FacturaElectronica.Dominio.Modelos;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Resumen;

/// <summary>
/// Validador para resumen de factura
/// </summary>
public interface IValidadorResumenFactura
{
    ResultadoValidacion Validar(Factura factura);
}
