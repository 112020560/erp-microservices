using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Entidades;
using MediatR;

namespace FacturaElectronica.Aplicacion.Webhooks;

public class TestWebhookCommandHandler(
    ITenantNotificationConfigRepository repository,
    IWebhookDispatcherService dispatcher)
    : IRequestHandler<TestWebhookCommand, TestWebhookResponse>
{
    public async Task<TestWebhookResponse> Handle(TestWebhookCommand request, CancellationToken cancellationToken)
    {
        var config = await repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (config is null || !config.IsActive)
            return new TestWebhookResponse(false, "No active notification configuration found for this tenant.");

        if (!config.Channel.HasFlag(NotificationChannel.Webhook))
            return new TestWebhookResponse(false, "Tenant notification channel is not configured for Webhook.");

        try
        {
            await dispatcher.DispatchAsync(request.TenantId, "webhook.test", new
            {
                Event = "webhook.test",
                Message = "This is a test webhook from FacturaElectronica",
                Timestamp = DateTime.UtcNow
            }, cancellationToken);

            return new TestWebhookResponse(true, "Test webhook sent successfully.");
        }
        catch (Exception ex)
        {
            return new TestWebhookResponse(false, $"Failed to send test webhook: {ex.Message}");
        }
    }
}
