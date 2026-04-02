using Catalogs.Infrastructure.Persistence;
using Catalogs.Infrastructure.Persistence.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Catalogs.Infrastructure.Messaging;

public sealed class OutboxDeliveryWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxDeliveryWorker> logger) : BackgroundService
{
    private const int MaxRetries = 5;
    private const int BatchSize = 50;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error in OutboxDeliveryWorker");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogsDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        var pending = await db.OutboxEvents
            .Where(e => e.Status == OutboxEventStatus.Pending && e.RetryCount < MaxRetries)
            .OrderBy(e => e.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
            return;

        foreach (var outboxEvent in pending)
        {
            try
            {
                var message = CatalogsEventMapper.ToMessage(outboxEvent.EventType, outboxEvent.Payload);

                if (message is null)
                {
                    logger.LogWarning("No mapper found for event type {EventType}", outboxEvent.EventType);
                    outboxEvent.Status = OutboxEventStatus.Failed;
                    outboxEvent.ProcessedOn = DateTimeOffset.UtcNow;
                    outboxEvent.Error = $"No mapper found for type '{outboxEvent.EventType}'";
                }
                else
                {
                    await bus.Publish(message, cancellationToken);
                    outboxEvent.Status = OutboxEventStatus.Delivered;
                    outboxEvent.ProcessedOn = DateTimeOffset.UtcNow;
                    logger.LogInformation("Published {EventType} {Id}", outboxEvent.EventType, outboxEvent.Id);
                }
            }
            catch (Exception ex)
            {
                outboxEvent.RetryCount++;
                outboxEvent.Error = ex.Message;

                if (outboxEvent.RetryCount >= MaxRetries)
                {
                    outboxEvent.Status = OutboxEventStatus.Failed;
                    outboxEvent.ProcessedOn = DateTimeOffset.UtcNow;
                    logger.LogError(ex, "Permanently failed {EventType} {Id} after {Max} retries",
                        outboxEvent.EventType, outboxEvent.Id, MaxRetries);
                }
                else
                {
                    logger.LogWarning(ex, "Failed to publish {EventType} {Id} — retry {Retry}/{Max}",
                        outboxEvent.EventType, outboxEvent.Id, outboxEvent.RetryCount, MaxRetries);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
