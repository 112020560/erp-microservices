using FacturaElectronica.Dominio.Modelos;
using FacturaElectronica.Dominio.Servicios.Validaciones.CodigosHacienda;

namespace FacturaElectronica.Dominio.Servicios.Validaciones.Emisores;

public class ValidadorEmisor : IValidadorEmisor
{
    private readonly IValidadorCodigosHacienda _validadorCodigos;

    public ValidadorEmisor(IValidadorCodigosHacienda validadorCodigos)
    {
        _validadorCodigos = validadorCodigos;
    }

    public ResultadoValidacion Validar(Factura factura)
    {
        var resultado = new ResultadoValidacion();

        // Campos obligatorios
        if (string.IsNullOrWhiteSpace(factura.EmisorNombre))
            resultado.AgregarError("El nombre del emisor es obligatorio");

        if (string.IsNullOrWhiteSpace(factura.EmisorNumeroIdentificacion))
            resultado.AgregarError("El número de identificación del emisor es obligatorio");

        if (string.IsNullOrWhiteSpace(factura.EmisorTipoIdentificacion))
            resultado.AgregarError("El tipo de identificación del emisor es obligatorio");

        // Validar tipo de identificación
        if (!string.IsNullOrEmpty(factura.EmisorTipoIdentificacion))
        {
            var validacion = _validadorCodigos.ValidarTipoIdentificacion(factura.EmisorTipoIdentificacion);
            if (!validacion.EsValido)
                resultado.Errores.AddRange(validacion.Errores);
        }

        // Validar formato de identificación
        if (!string.IsNullOrEmpty(factura.EmisorNumeroIdentificacion) && 
            !string.IsNullOrEmpty(factura.EmisorTipoIdentificacion))
        {
            var validacion = ValidarFormatoIdentificacion(factura.EmisorTipoIdentificacion, factura.EmisorNumeroIdentificacion);
            if (!validacion.EsValido)
                resultado.Errores.AddRange(validacion.Errores);
        }

        // Validar ubicación si está presente
        if (TieneUbicacionCompleta(factura))
        {
            var validacionUbicacion = ValidarUbicacionEmisor(factura);
            resultado.Errores.AddRange(validacionUbicacion.Errores);
        }

        // Validar correo electrónico si está presente
        if (!string.IsNullOrEmpty(factura.EmisorCorreoElectronico) && 
            !EsEmailValido(factura.EmisorCorreoElectronico))
        {
            resultado.AgregarError("El formato del correo electrónico del emisor no es válido");
        }

        return resultado;
    }

    private bool TieneUbicacionCompleta(Factura factura)
    {
        return !string.IsNullOrEmpty(factura.EmisorProvincia) ||
               !string.IsNullOrEmpty(factura.EmisorCanton) ||
               !string.IsNullOrEmpty(factura.EmisorDistrito) ||
               !string.IsNullOrEmpty(factura.EmisorBarrio);
    }

    private ResultadoValidacion ValidarUbicacionEmisor(Factura factura)
    {
        var resultado = new ResultadoValidacion();

        if (string.IsNullOrWhiteSpace(factura.EmisorProvincia))
            resultado.AgregarError("La provincia del emisor es requerida cuando se especifica ubicación");

        if (string.IsNullOrWhiteSpace(factura.EmisorCanton))
            resultado.AgregarError("El cantón del emisor es requerido cuando se especifica ubicación");

        if (string.IsNullOrWhiteSpace(factura.EmisorDistrito))
            resultado.AgregarError("El distrito del emisor es requerido cuando se especifica ubicación");

        if (string.IsNullOrWhiteSpace(factura.EmisorBarrio))
            resultado.AgregarError("El barrio del emisor es requerido cuando se especifica ubicación");

        return resultado;
    }

    private ResultadoValidacion ValidarFormatoIdentificacion(string tipoIdentificacion, string numeroIdentificacion)
    {
        var resultado = new ResultadoValidacion();

        switch (tipoIdentificacion)
        {
            case "01": // Cédula física
                if (!ValidarCedulaFisica(numeroIdentificacion))
                    resultado.AgregarError("El formato de la cédula física no es válido");
                break;

            case "02": // Cédula jurídica
                if (!ValidarCedulaJuridica(numeroIdentificacion))
                    resultado.AgregarError("El formato de la cédula jurídica no es válido");
                break;

            case "03": // DIMEX
                if (!ValidarDimex(numeroIdentificacion))
                    resultado.AgregarError("El formato del DIMEX no es válido");
                break;

            case "04": // NITE
                if (!ValidarNite(numeroIdentificacion))
                    resultado.AgregarError("El formato del NITE no es válido");
                break;
        }

        return resultado;
    }

    private bool ValidarCedulaFisica(string cedula)
    {
        if (string.IsNullOrEmpty(cedula) || cedula.Length != 9 || !cedula.All(char.IsDigit))
            return false;

        // Validar provincia (primeros dos dígitos)
        var provincia = int.Parse(cedula.Substring(0, 2));
        return provincia >= 1 && provincia <= 9;
    }

    private bool ValidarCedulaJuridica(string cedula)
    {
        if (string.IsNullOrEmpty(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit))
            return false;

        // Cédulas jurídicas empiezan con 3
        return cedula.StartsWith("3");
    }

    private bool ValidarDimex(string dimex)
    {
        if (string.IsNullOrEmpty(dimex) || dimex.Length != 11 || !dimex.All(char.IsDigit))
            return false;

        // DIMEX empieza con 1
        return dimex.StartsWith("1");
    }

    private bool ValidarNite(string nite)
    {
        if (string.IsNullOrEmpty(nite) || nite.Length != 10 || !nite.All(char.IsDigit))
            return false;

        // NITE empieza con 2
        return nite.StartsWith("2");
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