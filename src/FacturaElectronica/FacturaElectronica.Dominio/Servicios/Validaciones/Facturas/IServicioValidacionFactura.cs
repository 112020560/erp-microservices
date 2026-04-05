using FacturaElectronica.Dominio.Servicios.Factory;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Facturas;

/// <summary>
/// Servicio de validación principal que orquesta todas las validaciones
/// </summary>
public interface IServicioValidacionFactura
{
    Task<ResultadoValidacion> ValidarFacturaAsync(Modelos.Factura factura, VersionFacturaElectronica version = VersionFacturaElectronica.V44);
    Task<ResultadoValidacion> ValidarFacturaCompletaAsync(Modelos.Factura factura, VersionFacturaElectronica version = VersionFacturaElectronica.V44);
}