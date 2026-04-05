using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Servicios.Validaciones.CodigosHacienda;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Receptores;

public class ValidadorReceptor : IValidadorReceptor
{
    private readonly IValidadorCodigosHacienda _validadorCodigos;

    public ValidadorReceptor(IValidadorCodigosHacienda validadorCodigos)
    {
        _validadorCodigos = validadorCodigos;
    }

    public ResultadoValidacion Validar(Factura factura)
    {
        var resultado = new ResultadoValidacion();

        if (!factura.Receptor)
            return resultado;

        // Si tiene receptor, validar campos obligatorios
        if (string.IsNullOrWhiteSpace(factura.ReceptorNombre))
            resultado.AgregarError("El nombre del receptor es obligatorio");

        if (string.IsNullOrWhiteSpace(factura.ReceptorNumeroIdentificacion))
            resultado.AgregarError("El número de identificación del receptor es obligatorio");

        if (string.IsNullOrWhiteSpace(factura.ReceptorTipoIdentificacion))
            resultado.AgregarError("El tipo de identificación del receptor es obligatorio");

        // Validar tipo de identificación
        if (!string.IsNullOrEmpty(factura.ReceptorTipoIdentificacion))
        {
            var validacion = _validadorCodigos.ValidarTipoIdentificacion(factura.ReceptorTipoIdentificacion);
            if (!validacion.EsValido)
                resultado.Errores.AddRange(validacion.Errores);
        }

        // Validar correo si está presente
        if (!string.IsNullOrEmpty(factura.ReceptorCorreoElectronico) && 
            !EsEmailValido(factura.ReceptorCorreoElectronico))
        {
            resultado.AgregarError("El formato del correo electrónico del receptor no es válido");
        }

        return resultado;
    }

    private bool EsEmailValido(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}