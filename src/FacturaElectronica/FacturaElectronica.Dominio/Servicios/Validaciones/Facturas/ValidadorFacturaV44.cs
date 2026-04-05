using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Servicios.Validaciones.CodigosHacienda;
using FacturaElectronica.Dominio.Servicios.Validaciones.Emisores;
using FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Detalles;
using FacturaElectronica.Dominio.Servicios.Validaciones.Facturas.Resumen;
using FacturaElectronica.Dominio.Servicios.Validaciones.Receptores;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Facturas;

/// <summary>
/// Validador principal para facturas electrónicas v4.4
/// </summary>
public class ValidadorFacturaV44 : IValidador<Factura>
{
    private readonly IValidadorEmisor _validadorEmisor;
    private readonly IValidadorReceptor _validadorReceptor;
    private readonly IValidadorDetalleServicio _validadorDetalleServicio;
    private readonly IValidadorResumenFactura _validadorResumenFactura;
    private readonly IValidadorCodigosHacienda _validadorCodigos;

    public ValidadorFacturaV44(
        IValidadorEmisor validadorEmisor,
        IValidadorReceptor validadorReceptor,
        IValidadorDetalleServicio validadorDetalleServicio,
        IValidadorResumenFactura validadorResumenFactura,
        IValidadorCodigosHacienda validadorCodigos)
    {
        _validadorEmisor = validadorEmisor;
        _validadorReceptor = validadorReceptor;
        _validadorDetalleServicio = validadorDetalleServicio;
        _validadorResumenFactura = validadorResumenFactura;
        _validadorCodigos = validadorCodigos;
    }

    public ResultadoValidacion Validar(Factura factura)
    {
        var resultado = new ResultadoValidacion();

        // Validaciones básicas
        ValidarCamposObligatorios(factura, resultado);
        
        // Validaciones específicas v4.4
        ValidarVersionV44(factura, resultado);

        // Validaciones por secciones
        var resultadoEmisor = _validadorEmisor.Validar(factura);
        resultado.Errores.AddRange(resultadoEmisor.Errores);
        resultado.Advertencias.AddRange(resultadoEmisor.Advertencias);

        if (factura.Receptor)
        {
            var resultadoReceptor = _validadorReceptor.Validar(factura);
            resultado.Errores.AddRange(resultadoReceptor.Errores);
            resultado.Advertencias.AddRange(resultadoReceptor.Advertencias);
        }

        var resultadoDetalle = _validadorDetalleServicio.Validar(factura.DetalleServicios);
        resultado.Errores.AddRange(resultadoDetalle.Errores);
        resultado.Advertencias.AddRange(resultadoDetalle.Advertencias);

        var resultadoResumen = _validadorResumenFactura.Validar(factura);
        resultado.Errores.AddRange(resultadoResumen.Errores);
        resultado.Advertencias.AddRange(resultadoResumen.Advertencias);

        // Validar códigos de Hacienda
        var resultadoCodigos = _validadorCodigos.ValidarFactura(factura);
        resultado.Errores.AddRange(resultadoCodigos.Errores);
        resultado.Advertencias.AddRange(resultadoCodigos.Advertencias);

        return resultado;
    }

    private void ValidarCamposObligatorios(Factura factura, ResultadoValidacion resultado)
    {
        if (string.IsNullOrWhiteSpace(factura.TipoDocumento))
            resultado.AgregarError("El tipo de documento es obligatorio");

        if (factura.FechaDocumento == default)
            resultado.AgregarError("La fecha del documento es obligatoria");

        if (factura.ConsecutivoDocumento <= 0)
            resultado.AgregarError("El consecutivo del documento debe ser mayor a cero");
    }

    private void ValidarVersionV44(Factura factura, ResultadoValidacion resultado)
    {
        // Campo obligatorio en v4.4
        if (string.IsNullOrWhiteSpace(factura.CodigoActividad))
            resultado.AgregarError("El código de actividad económica (CIIU 4) es obligatorio en v4.4");

        // Validar fecha de emisión actual (v4.4 requiere fecha actual)
        var diferenciaDias = Math.Abs((DateTime.Now - factura.FechaDocumento).TotalDays);
        if (diferenciaDias > 1)
            resultado.AgregarAdvertencia("La fecha de emisión no corresponde a la fecha actual");

        // Validaciones específicas de moneda v4.4
        if (!string.IsNullOrEmpty(factura.CodigoMoneda) && factura.CodigoMoneda != "CRC")
        {
            if (string.IsNullOrEmpty(factura.TipoCambio) || !decimal.TryParse(factura.TipoCambio, out var tipoCambio) || tipoCambio <= 0)
                resultado.AgregarError("El tipo de cambio es obligatorio y debe ser mayor a cero cuando se usa moneda extranjera");
        }
    }
}