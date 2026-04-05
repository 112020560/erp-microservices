using SharedKernel.Contracts.ElectronicInvoice;

namespace FacturaElectronica.Aplicacion.Wrappers;

/// <summary>
/// ═══════════════════════════════════════════════════════════════════════════════
/// EXTENSIONES: Métodos helper para crear respuestas
/// ═══════════════════════════════════════════════════════════════════════════════
/// Ubicación: FacturaElectronica.Aplicacion/Wrappers/ResultadoFacturaElectronicaExtensions.cs
/// </summary>
public static class ResultadoFacturaElectronicaExtensions
{
    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static ResultadoFacturaElectronica Exitoso(string mensaje, object? data = null)
    {
        return new ResultadoFacturaElectronica
        {
            Exitoso = true,
            Mensaje = mensaje,
            Data = data,
            Errores = new List<string>(),
            Advertencias = new List<string>(),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crea una respuesta exitosa con advertencias
    /// </summary>
    public static ResultadoFacturaElectronica ExitosoConAdvertencias(
        string mensaje,
        IEnumerable<string> advertencias,
        object? data = null)
    {
        return new ResultadoFacturaElectronica
        {
            Exitoso = true,
            Mensaje = mensaje,
            Data = data,
            Errores = new List<string>(),
            Advertencias = advertencias.ToList(),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crea una respuesta con error
    /// </summary>
    public static ResultadoFacturaElectronica ConError(string mensaje, IEnumerable<string> errores)
    {
        return new ResultadoFacturaElectronica
        {
            Exitoso = false,
            Mensaje = mensaje,
            Errores = errores.ToList(),
            Advertencias = new List<string>(),
            Data = null,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crea una respuesta con un solo error
    /// </summary>
    public static ResultadoFacturaElectronica ConError(string mensaje, string error)
    {
        return ConError(mensaje, new[] { error });
    }

    /// <summary>
    /// Crea una respuesta de error desde una excepción
    /// </summary>
    public static ResultadoFacturaElectronica DesdeExcepcion(string mensaje, Exception exception)
    {
        var errores = new List<string>
        {
            exception.Message
        };

        // Agregar InnerException si existe
        if (exception.InnerException != null)
        {
            errores.Add($"Inner: {exception.InnerException.Message}");
        }

        return new ResultadoFacturaElectronica
        {
            Exitoso = false,
            Mensaje = mensaje,
            Errores = errores,
            Advertencias = new List<string>(),
            Data = new { ExceptionType = exception.GetType().Name },
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crea una respuesta de validación fallida
    /// </summary>
    public static ResultadoFacturaElectronica ValidacionFallida(
        IEnumerable<string> erroresValidacion,
        IEnumerable<string>? advertencias = null)
    {
        return new ResultadoFacturaElectronica
        {
            Exitoso = false,
            Mensaje = "La factura contiene errores de validación",
            Errores = erroresValidacion.ToList(),
            Advertencias = advertencias?.ToList() ?? new List<string>(),
            Data = null,
            Timestamp = DateTime.UtcNow,
            CodigoEstado = "VALIDACION_FALLIDA"
        };
    }
}