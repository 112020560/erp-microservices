using FacturaElectronica.Dominio.Modelos;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Detalles;

/// <summary>
/// Validador para detalle de servicios/productos
/// </summary>
public interface IValidadorDetalleServicio
{
    ResultadoValidacion Validar(List<DetalleServicio>? detalleServicios);
}