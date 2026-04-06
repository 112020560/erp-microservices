using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond;
using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Entidades;
using Microsoft.Extensions.Logging;

namespace FacturaElectronica.Infraestructura.Webhook;

public class WebhookDispatcherService(
    IHttpClientFactory httpClientFactory,
    ITenantNotificationConfigRepository repository,
    ILogger<WebhookDispatcherService> logger) : IWebhookDispatcherService
{
    public async Task DispatchAsync(Guid tenantId, string eventType, object payload, CancellationToken ct = default)
    {
        var config = await repository.GetByTenantIdAsync(tenantId, ct);

        if (config is null || !config.IsActive)
            return;

        if (!config.Channel.HasFlag(NotificationChannel.Webhook))
            return;

        if (string.IsNullOrWhiteSpace(config.WebhookUrl))
            return;

        var subscribedEvents = config.SubscribedEvents
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (!subscribedEvents.Contains(eventType) && !subscribedEvents.Contains("*"))
            return;

        var body = JsonSerializer.Serialize(new
        {
            Id = Guid.NewGuid().ToString(),
            Event = eventType,
            TenantId = tenantId,
            Timestamp = DateTime.UtcNow,
            Data = payload
        });

        var signature = ComputeHmacSignature(body, config.WebhookSecret ?? string.Empty);
        var deliveryId = Guid.NewGuid().ToString();

        using var client = httpClientFactory.CreateClient("webhook");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, config.WebhookUrl)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Add("X-Webhook-Signature", $"sha256={signature}");
        httpRequest.Headers.Add("X-Webhook-Event", eventType);
        httpRequest.Headers.Add("X-Webhook-Delivery", deliveryId);

        try
        {
            var response = await client.SendAsync(httpRequest, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Webhook delivery {DeliveryId} for tenant {TenantId} failed with status {Status}",
                    deliveryId, tenantId, response.StatusCode);
            }
            else
            {
                logger.LogInformation(
                    "Webhook delivery {DeliveryId} for tenant {TenantId} event {Event} delivered successfully",
                    deliveryId, tenantId, eventType);
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex,
                "Webhook delivery {DeliveryId} for tenant {TenantId} failed",
                deliveryId, tenantId);
        }
    }

    private static string ComputeHmacSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(payload);
        var hashBytes = HMACSHA256.HashData(keyBytes, messageBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}
