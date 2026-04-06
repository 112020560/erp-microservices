using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Entidades;
using MediatR;

namespace FacturaElectronica.Aplicacion.Webhooks;

public class UpsertNotificationConfigCommandHandler(
    ITenantNotificationConfigRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertNotificationConfigCommand, UpsertNotificationConfigResponse>
{
    public async Task<UpsertNotificationConfigResponse> Handle(
        UpsertNotificationConfigCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (existing is null)
        {
            existing = new TenantNotificationConfig
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Channel = request.Channel,
                WebhookUrl = request.WebhookUrl,
                WebhookSecret = request.WebhookSecret,
                IsActive = request.IsActive,
                SubscribedEvents = request.SubscribedEvents,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await repository.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.Channel = request.Channel;
            existing.WebhookUrl = request.WebhookUrl;
            existing.WebhookSecret = request.WebhookSecret;
            existing.IsActive = request.IsActive;
            existing.SubscribedEvents = request.SubscribedEvents;
            existing.UpdatedAt = DateTime.UtcNow;
            repository.Update(existing);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UpsertNotificationConfigResponse(existing.Id, existing.Channel, existing.IsActive);
    }
}
