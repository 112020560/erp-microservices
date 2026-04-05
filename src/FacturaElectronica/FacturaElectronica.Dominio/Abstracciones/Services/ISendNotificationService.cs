using FacturaElectronica.Dominio.Entidades;

namespace FacturaElectronica.Dominio.Abstracciones.Services;

/// <summary>
/// Servicio para enviar notificaciones de documentos electrónicos
/// </summary>
public interface ISendNotificationService
{
    /// <summary>
    /// Envía notificación al receptor cuando la factura es aceptada por Hacienda
    /// </summary>
    Task EnviarNotificacionFacturaAceptadaAsync(
        ElectronicInvoice invoice,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía notificación al receptor cuando la factura es rechazada por Hacienda
    /// </summary>
    Task EnviarNotificacionFacturaRechazadaAsync(
        ElectronicInvoice invoice,
        string motivoRechazo,
        CancellationToken cancellationToken = default);
}
