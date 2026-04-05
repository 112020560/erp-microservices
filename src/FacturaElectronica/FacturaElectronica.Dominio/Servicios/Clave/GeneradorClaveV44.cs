using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Dominio.Servicios.Clave;

/// <summary>
/// Generador de clave numérica según especificaciones oficiales de Hacienda Costa Rica
/// Estructura de 50 dígitos según Resolución DGT-R-48-2016
/// </summary>
public class GeneradorClaveV44 : IGeneradorClave
{
    private readonly ILogger<GeneradorClaveV44> _logger;
    private readonly Random _random;

    public GeneradorClaveV44(ILogger<GeneradorClaveV44> logger)
    {
        _logger = logger;
        _random = new Random();
    }

    /// <summary>
    /// Genera la clave numérica de 50 dígitos para cualquier tipo de comprobante
    /// </summary>
    /// <param name="numeroConsecutivo">Consecutivo completo de 20 dígitos (formato: 001-00001-01-0000000001)</param>
    /// <param name="cedulaEmisor">Número de identificación del emisor</param>
    /// <param>
    /// <param name="fechaEmision">Fecha de emisión del comprobante</param>
    /// <param name="tipoDocumento">Código del tipo de documento (01=Factura, 02=Nota Débito, 03=Nota Crédito, 04=Tiquete)</param>
    /// <param name="codigoSucursal">Código de sucursal (3 dígitos)</param>
    /// <param name="situacion">Situación del comprobante (1=Normal, 2=Contingencia, 3=Sin Internet)</param>
    /// <returns>Clave numérica de 50 dígitos</returns>
    public string GenerateInvoiceKey(
        long numeroConsecutivo,
        string cedulaEmisor,
        DateTime fechaEmision,
        string tipoDocumento,
        string codigoSucursal,
        string situacion)
    {
        try
        {
            // VALIDACIONES INICIALES
            ValidarParametros(numeroConsecutivo, cedulaEmisor, tipoDocumento, codigoSucursal, situacion);

            // ESTRUCTURA DE LA CLAVE (50 dígitos):
            // Posición  | Longitud | Descripción
            // -----------------------------------------
            // 1-3       | 3        | Código de país (506)
            // 4-5       | 2        | Día
            // 6-7       | 2        | Mes  
            // 8-9       | 2        | Año (últimos 2 dígitos)
            // 10-21     | 12       | Cédula del emisor
            // 22-41     | 20       | Consecutivo del comprobante
            // 42        | 1        | Situación
            // 43-50     | 8        | Código de seguridad
            // -----------------------------------------
            // TOTAL     | 50       | dígitos

            // 1. CÓDIGO DE PAÍS (3 dígitos): siempre 506 para Costa Rica
            string codigoPais = "506";

            // 2. FECHA (6 dígitos): ddmmaa
            string dia = fechaEmision.Day.ToString("D2");
            string mes = fechaEmision.Month.ToString("D2");
            string anio = fechaEmision.Year.ToString().Substring(2, 2); // Últimos 2 dígitos
            string fecha = dia + mes + anio; // Ejemplo: 290125 (29 de enero de 2025)

            // 3. CÉDULA EMISOR (12 dígitos): rellenar con ceros a la izquierda
            string cedulaLimpia = LimpiarIdentificacion(cedulaEmisor);
            string cedula = cedulaLimpia.PadLeft(12, '0'); // Ejemplo: 003101234567

            // 4. CONSECUTIVO (20 dígitos): número completo del consecutivo
            // Formato: SSS TTTTT TT NNNNNNNNNN
            // Donde:
            //   SSS = Sucursal (3 dígitos)
            //   TTTTT = Terminal/Punto de venta (5 dígitos)
            //   TT = Tipo de documento (2 dígitos)
            //   NNNNNNNNNN = Número consecutivo (10 dígitos)
            string consecutivoCompleto = GenerarConsecutivoCompleto(
                codigoSucursal, 
                "00001", // Terminal por defecto, ajustar según necesidad
                tipoDocumento, 
                numeroConsecutivo);

            // 5. SITUACIÓN (1 dígito): 1=Normal, 2=Contingencia, 3=Sin Internet
            string sit = situacion;

            // 6. CÓDIGO DE SEGURIDAD (8 dígitos): aleatorio
            string codigoSeguridad = GenerarCodigoSeguridad();

            // CONSTRUIR CLAVE COMPLETA (50 dígitos)
            string claveCompleta = codigoPais + fecha + cedula + consecutivoCompleto + sit + codigoSeguridad;

            // VALIDACIÓN FINAL
            if (claveCompleta.Length != 50)
            {
                throw new InvalidOperationException(
                    $"Error generando clave: longitud incorrecta. Esperado: 50, Actual: {claveCompleta.Length}");
            }

            if (!claveCompleta.All(char.IsDigit))
            {
                throw new InvalidOperationException("La clave debe contener solo dígitos");
            }

            _logger.LogDebug("Clave generada: {Clave} para consecutivo {Consecutivo}", 
                claveCompleta, numeroConsecutivo);

            return claveCompleta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando clave numérica para consecutivo {Consecutivo}", numeroConsecutivo);
            throw;
        }
    }

    /// <summary>
    /// Genera clave para nota de crédito (tipo documento 03)
    /// </summary>
    public string GenerateCreditNoteKey(
        long numeroConsecutivo,
        string cedulaEmisor,
        DateTime fechaEmision,
        string codigoSucursal,
        string situacion = "1")
    {
        return GenerateInvoiceKey(
            numeroConsecutivo,
            cedulaEmisor,
            fechaEmision,
            "03", // Tipo documento: Nota de Crédito
            codigoSucursal,
            situacion);
    }

    /// <summary>
    /// Genera clave para nota de débito (tipo documento 02)
    /// </summary>
    public string GenerateDebitNoteKey(
        long numeroConsecutivo,
        string cedulaEmisor,
        DateTime fechaEmision,
        string codigoSucursal,
        string situacion = "1")
    {
        return GenerateInvoiceKey(
            numeroConsecutivo,
            cedulaEmisor,
            fechaEmision,
            "02", // Tipo documento: Nota de Débito
            codigoSucursal,
            situacion);
    }

    /// <summary>
    /// Genera clave para tiquete electrónico (tipo documento 04)
    /// </summary>
    public string GenerateTicketKey(
        long numeroConsecutivo,
        string cedulaEmisor,
        DateTime fechaEmision,
        string codigoSucursal,
        string situacion = "1")
    {
        return GenerateInvoiceKey(
            numeroConsecutivo,
            cedulaEmisor,
            fechaEmision,
            "04", // Tipo documento: Tiquete Electrónico
            codigoSucursal,
            situacion);
    }

    /// <summary>
    /// Valida los parámetros de entrada
    /// </summary>
    private void ValidarParametros(
        long numeroConsecutivo,
        string cedulaEmisor,
        string tipoDocumento,
        string codigoSucursal,
        string situacion)
    {
        if (numeroConsecutivo <= 0)
            throw new ArgumentException("El número consecutivo debe ser mayor a cero");

        if (string.IsNullOrWhiteSpace(cedulaEmisor))
            throw new ArgumentNullException(nameof(cedulaEmisor), "La cédula del emisor es requerida");

        if (string.IsNullOrWhiteSpace(tipoDocumento) || tipoDocumento.Length != 2)
            throw new ArgumentException("El tipo de documento debe ser de 2 dígitos (01, 02, 03, 04, etc.)");

        if (string.IsNullOrWhiteSpace(codigoSucursal) || codigoSucursal.Length != 3)
            throw new ArgumentException("El código de sucursal debe ser de 3 dígitos");

        if (situacion != "1" && situacion != "2" && situacion != "3")
            throw new ArgumentException("La situación debe ser 1 (Normal), 2 (Contingencia) o 3 (Sin Internet)");

        // Validar tipos de documento conocidos
        var tiposValidos = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09" };
        if (!tiposValidos.Contains(tipoDocumento))
            _logger.LogWarning("Tipo de documento {TipoDocumento} no está en la lista de tipos conocidos", tipoDocumento);
    }

    /// <summary>
    /// Limpia la identificación removiendo guiones y espacios
    /// </summary>
    private string LimpiarIdentificacion(string identificacion)
    {
        return Regex.Replace(identificacion, @"[^0-9]", "");
    }

    /// <summary>
    /// Genera el consecutivo completo de 20 dígitos
    /// Formato: SSS TTTTT TT NNNNNNNNNN (sin espacios)
    /// </summary>
    private string GenerarConsecutivoCompleto(
        string sucursal,
        string terminal,
        string tipoDocumento,
        long numeroConsecutivo)
    {
        // Validar longitudes
        if (sucursal.Length != 3)
            throw new ArgumentException("Sucursal debe tener 3 dígitos");

        if (terminal.Length != 5)
            throw new ArgumentException("Terminal debe tener 5 dígitos");

        if (tipoDocumento.Length != 2)
            throw new ArgumentException("Tipo de documento debe tener 2 dígitos");

        // Número consecutivo: 10 dígitos
        string numero = numeroConsecutivo.ToString().PadLeft(10, '0');

        if (numero.Length > 10)
            throw new ArgumentException($"El número consecutivo {numeroConsecutivo} excede los 10 dígitos permitidos");

        // Concatenar todo: 3 + 5 + 2 + 10 = 20 dígitos
        return sucursal + terminal + tipoDocumento + numero;
    }

    /// <summary>
    /// Genera un código de seguridad aleatorio de 8 dígitos
    /// IMPORTANTE: En producción, este código debe ser único y tener
    /// una lógica que prevenga duplicados
    /// </summary>
    private string GenerarCodigoSeguridad()
    {
        // Generar número aleatorio de 8 dígitos
        // Rango: 10000000 a 99999999
        int codigoAleatorio = _random.Next(10000000, 99999999);
        
        return codigoAleatorio.ToString();
    }

    /// <summary>
    /// Descompone una clave numérica de 50 dígitos en sus componentes
    /// Útil para debugging y validaciones
    /// </summary>
    public ClaveDescompuesta DescomponerClave(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || clave.Length != 50)
            throw new ArgumentException("La clave debe tener exactamente 50 dígitos");

        if (!clave.All(char.IsDigit))
            throw new ArgumentException("La clave debe contener solo dígitos");

        return new ClaveDescompuesta
        {
            CodigoPais = clave.Substring(0, 3),
            Dia = clave.Substring(3, 2),
            Mes = clave.Substring(5, 2),
            Anio = clave.Substring(7, 2),
            CedulaEmisor = clave.Substring(9, 12),
            Consecutivo = clave.Substring(21, 20),
            Situacion = clave.Substring(41, 1),
            CodigoSeguridad = clave.Substring(42, 8),
            ClaveCompleta = clave
        };
    }
}

/// <summary>
/// Clase que representa una clave descompuesta
/// </summary>
public class ClaveDescompuesta
{
    public string CodigoPais { get; set; } = string.Empty;
    public string Dia { get; set; } = string.Empty;
    public string Mes { get; set; } = string.Empty;
    public string Anio { get; set; } = string.Empty;
    public string CedulaEmisor { get; set; } = string.Empty;
    public string Consecutivo { get; set; } = string.Empty;
    public string Situacion { get; set; } = string.Empty;
    public string CodigoSeguridad { get; set; } = string.Empty;
    public string ClaveCompleta { get; set; } = string.Empty;

    public DateTime ObtenerFecha()
    {
        int dia = int.Parse(Dia);
        int mes = int.Parse(Mes);
        int anio = 2000 + int.Parse(Anio); // Asumiendo años 2000+
        
        return new DateTime(anio, mes, dia);
    }

    public string ObtenerSucursal() => Consecutivo.Substring(0, 3);
    public string ObtenerTerminal() => Consecutivo.Substring(3, 5);
    public string ObtenerTipoDocumento() => Consecutivo.Substring(8, 2);
    public string ObtenerNumeroConsecutivo() => Consecutivo.Substring(10, 10);

    public override string ToString()
    {
        return $@"
Clave Descompuesta:
  País: {CodigoPais}
  Fecha: {Dia}/{Mes}/20{Anio}
  Cédula Emisor: {CedulaEmisor}
  Consecutivo:
    - Sucursal: {ObtenerSucursal()}
    - Terminal: {ObtenerTerminal()}
    - Tipo Doc: {ObtenerTipoDocumento()}
    - Número: {ObtenerNumeroConsecutivo()}
  Situación: {Situacion} ({(Situacion == "1" ? "Normal" : Situacion == "2" ? "Contingencia" : "Sin Internet")})
  Código Seguridad: {CodigoSeguridad}
  Clave Completa: {ClaveCompleta}";
    }
}