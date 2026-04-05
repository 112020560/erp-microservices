using FacturaElectronica.Dominio.Modelos;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Receptores;

/// <summary>
/// Validador específico para datos del receptor
/// </summary>
public interface IValidadorReceptor
{
    ResultadoValidacion Validar(Factura factura);
}