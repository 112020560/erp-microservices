using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using MediatR;

namespace FacturaElectronica.Aplicacion.Webhooks;

public class GetNotificationConfigQueryHandler(ITenantNotificationConfigRepository repository)
    : IRequestHandler<GetNotificationConfigQuery, NotificationConfigResponse?>
{
    public async Task<NotificationConfigResponse?> Handle(
        GetNotificationConfigQuery request, CancellationToken cancellationToken)
    {
        var config = await repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (config is null) return null;

        return new NotificationConfigResponse(
            config.Id,
            config.Channel,
            config.WebhookUrl,
            config.IsActive,
            config.SubscribedEvents,
            config.CreatedAt,
            config.UpdatedAt);
    }
}
