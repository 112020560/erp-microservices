using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Retail.Domain.Pricing.Abstractions;

namespace Retail.Infrastructure.Services;

internal sealed class ScheduledPriceChangeApplierService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ScheduledPriceChangeApplierService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ScheduledPriceChangeApplierService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ApplyDueChangesAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }

        logger.LogInformation("ScheduledPriceChangeApplierService stopped.");
    }

    private async Task ApplyDueChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();

            var scheduledRepo = scope.ServiceProvider.GetRequiredService<IScheduledPriceChangeRepository>();
            var priceListRepo = scope.ServiceProvider.GetRequiredService<IPriceListRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var now = DateTimeOffset.UtcNow;
            var dueChanges = await scheduledRepo.GetPendingDueAsync(now, cancellationToken);

            if (dueChanges.Count == 0) return;

            logger.LogInformation("Applying {Count} scheduled price changes.", dueChanges.Count);

            foreach (var change in dueChanges)
            {
                var priceList = await priceListRepo.GetByIdWithItemsAsync(change.PriceListId, cancellationToken);
                if (priceList is null)
                {
                    logger.LogWarning(
                        "PriceList {PriceListId} not found for scheduled change {ChangeId}. Skipping.",
                        change.PriceListId, change.Id);
                    continue;
                }

                var applyResult = priceList.ApplyScheduledItemChange(
                    change.ItemId,
                    change.NewPrice,
                    change.NewDiscountPercentage,
                    change.NewMinPrice);

                if (applyResult.IsFailure)
                {
                    logger.LogWarning(
                        "Failed to apply scheduled change {ChangeId} to item {ItemId}: {Error}",
                        change.Id, change.ItemId, applyResult.Error.Description);
                    continue;
                }

                var markResult = change.Apply();
                if (markResult.IsFailure)
                {
                    logger.LogWarning(
                        "Failed to mark scheduled change {ChangeId} as applied: {Error}",
                        change.Id, markResult.Error.Description);
                    continue;
                }

                priceListRepo.Update(priceList);
                scheduledRepo.Update(change);

                logger.LogInformation(
                    "Applied scheduled price change {ChangeId} to item {ItemId} in price list {PriceListId}.",
                    change.Id, change.ItemId, change.PriceListId);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while applying scheduled price changes.");
        }
    }
}
