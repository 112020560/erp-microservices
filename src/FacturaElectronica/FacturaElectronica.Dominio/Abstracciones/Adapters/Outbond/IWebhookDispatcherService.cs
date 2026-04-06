namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond;

public interface IWebhookDispatcherService
{
    Task DispatchAsync(Guid tenantId, string eventType, object payload, CancellationToken ct = default);
}
