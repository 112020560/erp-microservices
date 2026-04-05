using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Storage;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Web;
using FacturaElectronica.Dominio.Abstracciones.Services;
using FacturaElectronica.Dominio.Entidades;
using FacturaElectronica.Dominio.Modelos.Fiscal;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedKernel.Contracts.ElectronicInvoice;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.Polling;

public class PollingFacturasService : IPollingFacturasService
{
    private readonly ILogger<PollingFacturasService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServicioDocumentosHacienda _servicioDocumentosHacienda;
    private readonly IServicioAlmacenamientoDocumentos _storage;
    private readonly ISendNotificationService _notificationService;
    private readonly IPublishEndpoint _publishEndpoint;

    public PollingFacturasService(
        ILogger<PollingFacturasService> logger,
        IUnitOfWork unitOfWork,
        IServicioAlmacenamientoDocumentos storage,
        IServicioDocumentosHacienda servicioDocumentosHacienda,
        ISendNotificationService notificationService,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _storage = storage;
        _servicioDocumentosHacienda = servicioDocumentosHacienda;
        _notificationService = notificationService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando proceso de polling de facturas electrónicas.");

        // Obtener todos los tenants activos e iterar por cada uno
        var tenants = await _unitOfWork.TenantRepository.GetAllActiveAsync(cancellationToken);

        if (!tenants.Any())
        {
            _logger.LogInformation("No se encontraron tenants activos para polling.");
            return;
        }

        foreach (var tenant in tenants)
        {
            await ProcesarPollingParaTenant(tenant.Id, cancellationToken);
        }

        _logger.LogInformation("Proceso de polling de facturas electrónicas finalizado.");
    }

    private async Task ProcesarPollingParaTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Procesando polling para tenant {TenantId}", tenantId);

        var pendientes = await _unitOfWork.ElectronicInvoiceRepository.GetPendingProcessAsync(
            "enviado", tenantId, limit: 10, cancellationToken);

        if (pendientes is not { Count: > 0 })
        {
            _logger.LogDebug("No se encontraron facturas pendientes para tenant {TenantId}", tenantId);
            return;
        }

        _logger.LogInformation("Se encontraron {Count} facturas pendientes para tenant {TenantId}.",
            pendientes.Count, tenantId);

        foreach (var electronicInvoice in pendientes)
        {
            try
            {
                _logger.LogInformation("Procesando factura con clave: {Clave}", electronicInvoice.Clave);
                var consultaResponseDomain = await _servicioDocumentosHacienda.ConsultarDocumentoAsync(
                    electronicInvoice.Clave!, cancellationToken);

                if (consultaResponseDomain == null)
                {
                    _logger.LogWarning("No se recibió respuesta de Hacienda para la factura con clave: {Clave}", electronicInvoice.Clave);
                    continue;
                }

                // Actualizar el registro con la respuesta recibida
                await ActualizarRegistroConRespuesta(electronicInvoice, consultaResponseDomain, cancellationToken);

                if (!string.IsNullOrEmpty(consultaResponseDomain.RespuestaXml))
                {
                    await _storage.ActualizarConRespuestaAsync(electronicInvoice.Clave!, consultaResponseDomain.RespuestaXml, cancellationToken);
                }

                // Publicar evento de dominio
                await _publishEndpoint.Publish(new ElectronicDocumentProcessedEvent
                {
                    TenantId = electronicInvoice.TenantId,
                    DocumentId = electronicInvoice.Id,
                    ExternalDocumentId = electronicInvoice.ExternalDocumentId,
                    DocumentType = electronicInvoice.InvoiceType,
                    Status = consultaResponseDomain.IndicadorEstado ?? "desconocido",
                    Clave = electronicInvoice.Clave,
                    Consecutivo = electronicInvoice.Consecutivo,
                    ResponseMessage = consultaResponseDomain.IndicadorEstado,
                    ProcessedAt = DateTime.UtcNow
                }, cancellationToken);

                // Enviar notificación al receptor si está aceptada
                if (consultaResponseDomain.IndicadorEstado == "aceptado")
                {
                    _logger.LogInformation("Factura con clave: {Clave} fue aceptada por Hacienda.", electronicInvoice.Clave);

                    if (!electronicInvoice.NotificacionEnviada && !string.IsNullOrEmpty(electronicInvoice.CorreoReceptor))
                    {
                        await EnviarNotificacionAsync(electronicInvoice, null, cancellationToken);
                    }
                }
                else if (consultaResponseDomain.IndicadorEstado == "rechazado")
                {
                    _logger.LogWarning("Factura con clave: {Clave} fue rechazada por Hacienda.", electronicInvoice.Clave);

                    if (!electronicInvoice.NotificacionEnviada && !string.IsNullOrEmpty(electronicInvoice.CorreoReceptor))
                    {
                        var motivoRechazo = electronicInvoice.Error ?? "Documento rechazado por el Ministerio de Hacienda";
                        await EnviarNotificacionAsync(electronicInvoice, motivoRechazo, cancellationToken);
                    }
                }

                _logger.LogInformation("Factura con clave: {Clave} procesada correctamente.", electronicInvoice.Clave);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando la factura con clave: {Clave}", electronicInvoice.Clave);
            }
        }
    }

    private async Task EnviarNotificacionAsync(
        ElectronicInvoice invoice,
        string? motivoRechazo,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(motivoRechazo))
            {
                await _notificationService.EnviarNotificacionFacturaAceptadaAsync(invoice, cancellationToken);
            }
            else
            {
                await _notificationService.EnviarNotificacionFacturaRechazadaAsync(invoice, motivoRechazo, cancellationToken);
            }

            invoice.NotificacionEnviada = true;
            invoice.FechaNotificacion = DateTime.UtcNow;

            await _unitOfWork.ElectronicInvoiceRepository.UpdateAsync(invoice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Notificación enviada para factura {Clave} a {Correo}",
                invoice.Clave,
                invoice.CorreoReceptor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al enviar notificación para factura {Clave}. Se reintentará en el próximo ciclo.",
                invoice.Clave);
        }
    }

    private async Task ActualizarRegistroConRespuesta(
        ElectronicInvoice registroExistente,
        ConsultaDocumentoResponse respuestaHacienda,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Actualizando registro {Id} con respuesta de Hacienda", registroExistente.Id);

        var estado = respuestaHacienda.IndicadorEstado switch
        {
            "aceptado" => "aceptado",
            "rechazado" => "rechazado",
            "error" => "error",
            "desconocido" => "reintentando",
            _ => "desconocido"
        };

        registroExistente.Status = estado;
        registroExistente.StatusDetail = respuestaHacienda.IndicadorEstado switch
        {
            "aceptado" => "La factura ha sido aceptada por Hacienda.",
            "rechazado" => "La factura ha sido rechazada por Hacienda.",
            "error" => "Hubo un error en el procesamiento de la factura.",
            "desconocido" => "El estado de la factura es desconocido.",
            _ => "Estado no identificado."
        };

        registroExistente.ResponseMessage = respuestaHacienda.IndicadorEstado ?? "Sin indicador";

        if (estado == "rechazado" || estado == "error")
        {
            registroExistente.Error = $"{estado.ToUpper()}: {respuestaHacienda.IndicadorEstado}";
        }
        else
        {
            registroExistente.Error = null;
        }

        registroExistente.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ElectronicInvoiceRepository.UpdateAsync(registroExistente, cancellationToken);

        var logRespuesta = new ElectronicDocumentLog
        {
            DocumentId = registroExistente.Id,
            Action = estado,
            Message = "Respuesta recibida de Hacienda",
            Details = $"Estado: {estado}, Indicador: {respuestaHacienda.IndicadorEstado}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ElectronicDocumentLogRepository.AddLogAsync(logRespuesta, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registro {Id} actualizado exitosamente con estado {Estado}",
            registroExistente.Id, estado);
    }
}
