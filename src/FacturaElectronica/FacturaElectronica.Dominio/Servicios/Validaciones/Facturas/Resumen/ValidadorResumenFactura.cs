using FacturaElectronica.Dominio.Modelos;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Resumen;

public class ValidadorResumenFactura : IValidadorResumenFactura
{
    public ResultadoValidacion Validar(Factura factura)
    {
        var resultado = new ResultadoValidacion();

        // Validar que los totales no sean negativos
        if (factura.TotalVenta < 0)
            resultado.AgregarError("El total de venta no puede ser negativo");

        if (factura.TotalDescuentos < 0)
            resultado.AgregarError("El total de descuentos no puede ser negativo");

        if (factura.TotalImpuesto < 0)
            resultado.AgregarError("El total de impuesto no puede ser negativo");

        if (factura.TotalComprobante < 0)
            resultado.AgregarError("El total del comprobante no puede ser negativo");

        // Validar cálculos del resumen
        ValidarCalculosResumen(factura, resultado);

        return resultado;
    }

    private void ValidarCalculosResumen(Factura factura, ResultadoValidacion resultado)
    {
        // Calcular totales basados en las líneas de detalle
        if (factura.DetalleServicios != null && factura.DetalleServicios.Any())
        {
            var totalVentaCalculado = factura.DetalleServicios.Sum(d => d.Precio);
            var totalDescuentosCalculado = factura.DetalleServicios.Sum(d => d.Descuento);
            var totalImpuestoCalculado = factura.DetalleServicios.Sum(d => d.MontoImpuesto);
            var totalComprobanteCalculado = factura.DetalleServicios.Sum(d => d.MontoTotalLinea);

            if (Math.Abs(factura.TotalVenta - totalVentaCalculado) > 0.01m)
                resultado.AgregarError("El total de venta no coincide con la suma de las líneas de detalle");

            if (Math.Abs(factura.TotalDescuentos - totalDescuentosCalculado) > 0.01m)
                resultado.AgregarError("El total de descuentos no coincide con la suma de las líneas de detalle");

            if (Math.Abs(factura.TotalImpuesto - totalImpuestoCalculado) > 0.01m)
                resultado.AgregarError("El total de impuesto no coincide con la suma de las líneas de detalle");

            if (Math.Abs(factura.TotalComprobante - totalComprobanteCalculado) > 0.01m)
                resultado.AgregarError("El total del comprobante no coincide con la suma de las líneas de detalle");
        }

        // Validar fórmula: Total Comprobante = Total Venta Neta + Total Impuesto
        var totalComprobanteEsperado = factura.TotalVentaNeta + factura.TotalImpuesto;
        if (Math.Abs(factura.TotalComprobante - totalComprobanteEsperado) > 0.01m)
            resultado.AgregarError("El total del comprobante no coincide con la fórmula: Total Venta Neta + Total Impuesto");

        // Validar fórmula: Total Venta Neta = Total Venta - Total Descuentos
        var totalVentaNetaEsperado = factura.TotalVenta - factura.TotalDescuentos;
        if (Math.Abs(factura.TotalVentaNeta - totalVentaNetaEsperado) > 0.01m)
            resultado.AgregarError("El total de venta neta no coincide con la fórmula: Total Venta - Total Descuentos");
    }
}