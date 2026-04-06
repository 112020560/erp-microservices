using FacturaElectronica.Dominio.Entidades;
using MediatR;

namespace FacturaElectronica.Aplicacion.Webhooks;

public record GetNotificationConfigQuery(Guid TenantId) : IRequest<NotificationConfigResponse?>;

public record NotificationConfigResponse(
    Guid Id,
    NotificationChannel Channel,
    string? WebhookUrl,
    bool IsActive,
    string SubscribedEvents,
    DateTime CreatedAt,
    DateTime UpdatedAt);
