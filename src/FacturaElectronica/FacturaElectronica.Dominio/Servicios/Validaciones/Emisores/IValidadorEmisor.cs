using FacturaElectronica.Dominio.Modelos;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Emisores;

/// <summary>
/// Validador específico para datos del emisor
/// </summary>
public interface IValidadorEmisor
{
    ResultadoValidacion Validar(Factura factura);
}