using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Servicios.Validaciones.CodigosHacienda;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Detalles;

public class ValidadorDetalleServicio : IValidadorDetalleServicio
{
    private readonly IValidadorCodigosHacienda _validadorCodigos;

    public ValidadorDetalleServicio(IValidadorCodigosHacienda validadorCodigos)
    {
        _validadorCodigos = validadorCodigos;
    }

    public ResultadoValidacion Validar(List<DetalleServicio>? detalleServicios)
    {
        var resultado = new ResultadoValidacion();

        if (detalleServicios == null || !detalleServicios.Any())
        {
            resultado.AgregarError("Debe incluir al menos una línea de detalle");
            return resultado;
        }

        for (int i = 0; i < detalleServicios.Count; i++)
        {
            var detalle = detalleServicios[i];
            var prefijo = $"Línea {i + 1}: ";

            // Validar número de línea correlativo
            if (detalle.NumeroLinea != i + 1)
                resultado.AgregarError($"{prefijo}El número de línea debe ser correlativo");

            // Campos obligatorios
            if (string.IsNullOrWhiteSpace(detalle.DetalleArticulo))
                resultado.AgregarError($"{prefijo}La descripción es obligatoria");

            if (detalle.Cantidad <= 0)
                resultado.AgregarError($"{prefijo}La cantidad debe ser mayor a cero");

            if (detalle.PrecioUnitario < 0)
                resultado.AgregarError($"{prefijo}El precio unitario no puede ser negativo");

            // Validar unidad de medida
            if (!string.IsNullOrEmpty(detalle.UnidadMedida))
            {
                var validacionUM = _validadorCodigos.ValidarUnidadMedida(detalle.UnidadMedida);
                if (!validacionUM.EsValido)
                    resultado.AgregarError($"{prefijo}Unidad de medida inválida: {detalle.UnidadMedida}");
            }

            // Validar código de artículo (CABYS 2025)
            if (!string.IsNullOrEmpty(detalle.CodigoArticulo))
            {
                var validacionCABYS = _validadorCodigos.ValidarCodigoCABYS(detalle.CodigoArticulo);
                if (!validacionCABYS.EsValido)
                    resultado.AgregarAdvertencia($"{prefijo}Código CABYS podría no ser válido: {detalle.CodigoArticulo}");
            }

            // Validar cálculos
            ValidarCalculosLinea(detalle, resultado, prefijo);

            // Validar impuestos v4.4
            ValidarImpuestosV44(detalle, resultado, prefijo);
        }

        return resultado;
    }

    private void ValidarCalculosLinea(DetalleServicio detalle, ResultadoValidacion resultado, string prefijo)
    {
        var montoTotalCalculado = detalle.Cantidad * detalle.PrecioUnitario;
        if (Math.Abs(detalle.Precio - montoTotalCalculado) > 0.01m)
            resultado.AgregarError($"{prefijo}El monto total no coincide con cantidad × precio unitario");

        var subTotalCalculado = detalle.Precio - detalle.Descuento;
        if (Math.Abs(detalle.SubTotal - subTotalCalculado) > 0.01m)
            resultado.AgregarError($"{prefijo}El subtotal no coincide con monto total - descuento");

        var montoTotalLineaCalculado = detalle.SubTotal + detalle.MontoImpuesto;
        if (Math.Abs(detalle.MontoTotalLinea - montoTotalLineaCalculado) > 0.01m)
            resultado.AgregarError($"{prefijo}El monto total de línea no coincide con subtotal + impuesto");
    }

    private void ValidarImpuestosV44(DetalleServicio detalle, ResultadoValidacion resultado, string prefijo)
    {
        // Validar código de impuesto
        if (!string.IsNullOrEmpty(detalle.CodigoImpuesto))
        {
            var validacion = _validadorCodigos.ValidarCodigoImpuesto(detalle.CodigoImpuesto);
            if (!validacion.EsValido)
                resultado.AgregarError($"{prefijo}Código de impuesto inválido: {detalle.CodigoImpuesto}");
        }

        // Validar tarifa de impuesto
        if (detalle.TarifaImpuesto < 0 || detalle.TarifaImpuesto > 100)
            resultado.AgregarError($"{prefijo}La tarifa de impuesto debe estar entre 0 y 100");

        // Validar cálculo de impuesto
        var impuestoCalculado = detalle.SubTotal * (detalle.TarifaImpuesto / 100);
        if (Math.Abs(detalle.MontoImpuesto - impuestoCalculado) > 0.01m)
            resultado.AgregarAdvertencia($"{prefijo}El monto de impuesto calculado no coincide con el especificado");
    }
}