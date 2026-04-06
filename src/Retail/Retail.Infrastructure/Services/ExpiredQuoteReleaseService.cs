using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Retail.Domain.Pricing.Abstractions;
using Retail.Domain.Sales.Abstractions;
using SharedKernel.Contracts.Sales;

namespace Retail.Infrastructure.Services;

internal sealed class ExpiredQuoteReleaseService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ExpiredQuoteReleaseService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExpiredQuoteReleaseService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ExpireQuotesAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }

        logger.LogInformation("ExpiredQuoteReleaseService stopped.");
    }

    private async Task ExpireQuotesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();

            var quoteRepository = scope.ServiceProvider.GetRequiredService<ISaleQuoteRepository>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var now = DateTimeOffset.UtcNow;
            var expiredQuotes = await quoteRepository.GetExpiredOpenAsync(now, cancellationToken);

            if (expiredQuotes.Count == 0) return;

            logger.LogInformation("Expiring {Count} open quotes.", expiredQuotes.Count);

            foreach (var quote in expiredQuotes)
            {
                var result = quote.Expire();
                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "Failed to expire quote {QuoteId}: {Error}",
                        quote.Id, result.Error.Description);
                    continue;
                }

                quoteRepository.Update(quote);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var quote in expiredQuotes)
            {
                await publishEndpoint.Publish(new SaleQuoteExpiredEvent
                {
                    QuoteId = quote.Id,
                    QuoteNumber = quote.QuoteNumber,
                    ExpiredAt = now
                }, cancellationToken);
            }

            logger.LogInformation("Expired {Count} quotes successfully.", expiredQuotes.Count);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while expiring quotes.");
        }
    }
}
