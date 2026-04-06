using FacturaElectronica.Dominio.Entidades;
using MediatR;

namespace FacturaElectronica.Aplicacion.Webhooks;

public record UpsertNotificationConfigCommand(
    Guid TenantId,
    NotificationChannel Channel,
    string? WebhookUrl,
    string? WebhookSecret,
    bool IsActive,
    string SubscribedEvents) : IRequest<UpsertNotificationConfigResponse>;

public record UpsertNotificationConfigResponse(Guid Id, NotificationChannel Channel, bool IsActive);
