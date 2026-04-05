using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Abstracciones.Services;
using FacturaElectronica.Dominio.Entidades;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Aplicacion.Services;

public class SendNotificationService : ISendNotificationService
{
    private readonly IBus _bus;
    private readonly IServicioAlmacenamientoDocumentos _storageService;
    private readonly ILogger<SendNotificationService> _logger;

    public SendNotificationService(
        IBus bus,
        IServicioAlmacenamientoDocumentos storageService,
        ILogger<SendNotificationService> logger)
    {
        _bus = bus;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task EnviarNotificacionFacturaAceptadaAsync(
        ElectronicInvoice invoice,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(invoice.CorreoReceptor))
        {
            _logger.LogWarning(
                "No se puede enviar notificación para factura {Clave}: no hay correo del receptor",
                invoice.Clave);
            return;
        }

        try
        {
            // Obtener rutas de los documentos
            var documento = await _storageService.ObtenerDocumentoAsync(invoice.Clave, cancellationToken);
            if (documento == null)
            {
                _logger.LogWarning(
                    "No se encontraron documentos almacenados para la clave {Clave}",
                    invoice.Clave);
                return;
            }

            // Construir lista de adjuntos
            var adjuntos = new List<string>();

            if (!string.IsNullOrEmpty(documento.RutaFirmado))
                adjuntos.Add(documento.RutaFirmado);

            if (!string.IsNullOrEmpty(documento.RutaRespuesta))
                adjuntos.Add(documento.RutaRespuesta);

            // Construir el cuerpo del email
            var cuerpoHtml = ConstruirCuerpoFacturaAceptada(invoice);

            // Crear contrato de notificación
            //TODO:: Envio de  correo con la factura aceptada, se debe enviar el XML firmado y la respuesta de Hacienda como adjuntos. Se puede usar el mismo contrato de notificación pero con una propiedad adicional para indicar los adjuntos.
            // var notificacion = new NotificationContract
            // {
            //     Metodo = "EMAIL",
            //     Asunto_Titulo = $"Factura Electrónica {invoice.Consecutivo} - Aceptada por Hacienda",
            //     Correos = new[] { invoice.CorreoReceptor },
            //     CuerpoMensaje = cuerpoHtml,
            //     EsCuerpoHtml = true,
            //     RutasAdjuntos = adjuntos.ToArray()
            // };

            // Agregar teléfono si está disponible (para futuro SMS)
            // if (!string.IsNullOrEmpty(invoice.TelefonoReceptor))
            // {
            //     notificacion.Telefonos = new[] { invoice.TelefonoReceptor };
            // }

            // // Publicar a la cola de notificaciones
            // await _bus.Publish(notificacion, cancellationToken);

            _logger.LogInformation(
                "Notificación de factura aceptada publicada para {Clave} a {Correo} con {CantidadAdjuntos} adjunto(s)",
                invoice.Clave,
                invoice.CorreoReceptor,
                adjuntos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al enviar notificación para factura {Clave}",
                invoice.Clave);
            throw;
        }
    }

    public async Task EnviarNotificacionFacturaRechazadaAsync(
        ElectronicInvoice invoice,
        string motivoRechazo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(invoice.CorreoReceptor))
        {
            _logger.LogWarning(
                "No se puede enviar notificación de rechazo para factura {Clave}: no hay correo del receptor",
                invoice.Clave);
            return;
        }

        try
        {
            var cuerpoHtml = ConstruirCuerpoFacturaRechazada(invoice, motivoRechazo);

            //TODO:: Envio de correo con la factura rechazada, se debe enviar el motivo del rechazo en el cuerpo del mensaje. Se puede usar el mismo contrato de notificación pero sin adjuntos.
            // var notificacion = new NotificationContract
            // {
            //     Metodo = "EMAIL",
            //     Asunto_Titulo = $"Factura Electrónica {invoice.Consecutivo} - Rechazada por Hacienda",
            //     Correos = new[] { invoice.CorreoReceptor },
            //     CuerpoMensaje = cuerpoHtml,
            //     EsCuerpoHtml = true
            // };

            // await _bus.Publish(notificacion, cancellationToken);

            _logger.LogInformation(
                "Notificación de factura rechazada publicada para {Clave} a {Correo}",
                invoice.Clave,
                invoice.CorreoReceptor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al enviar notificación de rechazo para factura {Clave}",
                invoice.Clave);
            throw;
        }
    }

    private static string ConstruirCuerpoFacturaAceptada(ElectronicInvoice invoice)
    {
        var nombreReceptor = !string.IsNullOrEmpty(invoice.NombreReceptor)
            ? invoice.NombreReceptor
            : "Estimado cliente";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .details {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #28a745; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Factura Electrónica Aceptada</h1>
        </div>
        <div class='content'>
            <p>Hola {nombreReceptor},</p>
            <p>Le informamos que su factura electrónica ha sido <strong>aceptada</strong> por el Ministerio de Hacienda.</p>

            <div class='details'>
                <p><strong>Consecutivo:</strong> {invoice.Consecutivo}</p>
                <p><strong>Clave:</strong> {invoice.Clave}</p>
                <p><strong>Fecha de Emisión:</strong> {invoice.FechaEmision:dd/MM/yyyy HH:mm}</p>
                <p><strong>Tipo de Documento:</strong> {ObtenerTipoDocumento(invoice.InvoiceType)}</p>
            </div>

            <p>Adjunto encontrará:</p>
            <ul>
                <li>XML firmado del comprobante electrónico</li>
                <li>Respuesta de aceptación del Ministerio de Hacienda</li>
            </ul>

            <p>Guarde estos documentos para sus registros.</p>
        </div>
        <div class='footer'>
            <p>Este es un correo automático, por favor no responda a este mensaje.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string ConstruirCuerpoFacturaRechazada(ElectronicInvoice invoice, string motivoRechazo)
    {
        var nombreReceptor = !string.IsNullOrEmpty(invoice.NombreReceptor)
            ? invoice.NombreReceptor
            : "Estimado cliente";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .details {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #dc3545; }}
        .motivo {{ background-color: #fff3cd; padding: 15px; margin: 15px 0; border: 1px solid #ffc107; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Factura Electrónica Rechazada</h1>
        </div>
        <div class='content'>
            <p>Hola {nombreReceptor},</p>
            <p>Le informamos que su factura electrónica ha sido <strong>rechazada</strong> por el Ministerio de Hacienda.</p>

            <div class='details'>
                <p><strong>Consecutivo:</strong> {invoice.Consecutivo}</p>
                <p><strong>Clave:</strong> {invoice.Clave}</p>
                <p><strong>Fecha de Emisión:</strong> {invoice.FechaEmision:dd/MM/yyyy HH:mm}</p>
            </div>

            <div class='motivo'>
                <p><strong>Motivo del rechazo:</strong></p>
                <p>{motivoRechazo}</p>
            </div>

            <p>Por favor contacte al emisor para más información.</p>
        </div>
        <div class='footer'>
            <p>Este es un correo automático, por favor no responda a este mensaje.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string ObtenerTipoDocumento(string invoiceType)
    {
        return invoiceType switch
        {
            "01" => "Factura Electrónica",
            "02" => "Nota de Débito",
            "03" => "Nota de Crédito",
            "04" => "Tiquete Electrónico",
            "08" => "Comprobante en Contingencia",
            "09" => "Factura de Compra",
            _ => invoiceType
        };
    }
}
