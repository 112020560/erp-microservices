using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Servicios.Validaciones.CodigosHacienda;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Negocio;

/// <summary>
/// Validador especializado para casos de negocio específicos de v4.4
/// </summary>
public class ValidadorNegocioV44 : IValidador<Factura>
{
    private readonly ILogger<ValidadorNegocioV44> _logger;
    private readonly IValidadorCodigosHacienda _validadorCodigos;

    public ValidadorNegocioV44(ILogger<ValidadorNegocioV44> logger, IValidadorCodigosHacienda validadorCodigos)
    {
        _logger = logger;
        _validadorCodigos = validadorCodigos;
    }

    public ResultadoValidacion Validar(Factura factura)
    {
        var resultado = new ResultadoValidacion();

        // Validaciones específicas de negocio para v4.4
        ValidarReglasNegocioEspecificas(factura, resultado);
        ValidarConsistenciaDatos(factura, resultado);
        ValidarRequisitosLegales(factura, resultado);

        return resultado;
    }

    private void ValidarReglasNegocioEspecificas(Factura factura, ResultadoValidacion resultado)
    {
        // Regla: Si condición de venta es crédito, debe especificar plazo
        if (factura.CondicionVenta == "02" && (factura.PlazoCredito == null || factura.PlazoCredito <= 0))
        {
            resultado.AgregarError("Cuando la condición de venta es crédito, debe especificar el plazo de crédito");
        }

        // Regla: Si hay receptor, debe tener identificación válida
        if (factura.Receptor && string.IsNullOrEmpty(factura.ReceptorNumeroIdentificacion))
        {
            resultado.AgregarError("Si especifica receptor, debe incluir su identificación");
        }

        // Regla: Montos deben ser consistentes con la moneda
        if (factura.CodigoMoneda != "CRC" && string.IsNullOrEmpty(factura.TipoCambio))
        {
            resultado.AgregarError("Para moneda extranjera debe especificar tipo de cambio");
        }

        // Regla v4.4: Factura debe tener código de actividad
        if (string.IsNullOrEmpty(factura.CodigoActividad))
        {
            resultado.AgregarError("El código de actividad económica (CIIU 4) es obligatorio en v4.4");
        }
    }

    private void ValidarConsistenciaDatos(Factura factura, ResultadoValidacion resultado)
    {
        // Validar que los totales sean matemáticamente correctos
        var totalGravadoCalculado = factura.TotalServGravados + factura.TotalMercanciasGravadas;
        if (Math.Abs(factura.TotalGravado - totalGravadoCalculado) > 0.01m)
        {
            resultado.AgregarError("El total gravado no coincide con la suma de servicios y mercancías gravados");
        }

        var totalExentoCalculado = factura.TotalServExentos + factura.TotalMercanciasExentas;
        if (Math.Abs(factura.TotalExento - totalExentoCalculado) > 0.01m)
        {
            resultado.AgregarError("El total exento no coincide con la suma de servicios y mercancías exentos");
        }

        // Validar que la fecha de emisión sea reciente
        var diferenciaDias = (DateTime.Now - factura.FechaDocumento).TotalDays;
        if (diferenciaDias > 5)
        {
            resultado.AgregarAdvertencia("La fecha de emisión es muy antigua");
        }
        else if (diferenciaDias < -1)
        {
            resultado.AgregarError("La fecha de emisión no puede ser futura");
        }
    }

    private void ValidarRequisitosLegales(Factura factura, ResultadoValidacion resultado)
    {
        // Validar número de resolución
        if (string.IsNullOrEmpty(factura.NumeroResolucion))
        {
            resultado.AgregarError("El número de resolución es obligatorio");
        }

        // Validar fecha de resolución
        if (string.IsNullOrEmpty(factura.FechaResolucion))
        {
            resultado.AgregarError("La fecha de resolución es obligatoria");
        }
        else
        {
            if (DateTime.TryParse(factura.FechaResolucion, out var fechaResolucion))
            {
                if (fechaResolucion > DateTime.Now)
                {
                    resultado.AgregarError("La fecha de resolución no puede ser futura");
                }
            }
            else
            {
                resultado.AgregarError("El formato de fecha de resolución no es válido");
            }
        }

        // Validación específica v4.4: Impuestos más detallados
        ValidarImpuestosDetallados(factura, resultado);
    }

    private void ValidarImpuestosDetallados(Factura factura, ResultadoValidacion resultado)
    {
        if (factura.DetalleServicios == null) return;

        foreach (var detalle in factura.DetalleServicios)
        {
            // Validar que si hay impuesto, tenga código válido
            if (detalle.MontoImpuesto > 0 && string.IsNullOrEmpty(detalle.CodigoImpuesto))
            {
                resultado.AgregarError($"Línea {detalle.NumeroLinea}: Si hay monto de impuesto, debe especificar el código");
            }

            // Validar tarifa de impuesto específica para v4.4
            if (!string.IsNullOrEmpty(detalle.CodigoImpuesto) && detalle.CodigoImpuesto == "01") // IVA
            {
                var tarifasIVAValidas = new[] { 0m, 1m, 2m, 4m, 8m, 13m };
                if (!tarifasIVAValidas.Contains(detalle.TarifaImpuesto))
                {
                    resultado.AgregarError($"Línea {detalle.NumeroLinea}: Tarifa de IVA no válida: {detalle.TarifaImpuesto}%. Tarifas válidas: {string.Join(", ", tarifasIVAValidas)}%");
                }
            }
        }
    }
}