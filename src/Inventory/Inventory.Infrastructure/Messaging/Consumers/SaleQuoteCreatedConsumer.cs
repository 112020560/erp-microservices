using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Abstractions.Services;
using Inventory.Domain.Stock;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedKernel.Contracts.Sales;

namespace Inventory.Infrastructure.Messaging.Consumers;

public sealed class SaleQuoteCreatedConsumer(
    IStockEntryRepository stockEntryRepository,
    IStockReservationRepository reservationRepository,
    IMovementNumberGenerator numberGenerator,
    IUnitOfWork unitOfWork,
    ILogger<SaleQuoteCreatedConsumer> logger) : IConsumer<SaleQuoteCreatedEvent>
{
    public async Task Consume(ConsumeContext<SaleQuoteCreatedEvent> context)
    {
        var msg = context.Message;

        // Idempotency: check if reservations already exist for this quoteId
        var existing = await reservationRepository.GetBySalesOrderAsync(msg.QuoteId, context.CancellationToken);
        if (existing.Count > 0) return; // already processed

        foreach (var line in msg.Lines)
        {
            // Find stock entry for this product in this warehouse
            var stockEntries = await stockEntryRepository.GetByProductAsync(line.ProductId, context.CancellationToken);
            var stockEntry = stockEntries.FirstOrDefault(se => se.WarehouseId == msg.WarehouseId);

            if (stockEntry is null)
            {
                logger.LogWarning(
                    "No stock entry found for product {ProductId} in warehouse {WarehouseId} — skipping line for quote {QuoteId}",
                    line.ProductId, msg.WarehouseId, msg.QuoteId);
                continue;
            }

            // Reserve on StockEntry
            var reserveResult = stockEntry.Reserve(line.Quantity);
            if (reserveResult.IsFailure)
            {
                logger.LogWarning(
                    "Insufficient stock for product {ProductId} in warehouse {WarehouseId} (requested {Quantity}) — skipping line for quote {QuoteId}",
                    line.ProductId, msg.WarehouseId, line.Quantity, msg.QuoteId);
                continue;
            }

            stockEntryRepository.Update(stockEntry);

            // Create StockReservation record
            var reservationNumber = await numberGenerator.GenerateAsync("RSV", context.CancellationToken);
            var reservationResult = StockReservation.Create(
                reservationNumber,
                line.ProductId,
                msg.WarehouseId,
                stockEntry.LocationId,
                stockEntry.LotId,
                line.Quantity,
                msg.QuoteId,
                msg.ValidUntil);

            if (reservationResult.IsSuccess)
                reservationRepository.Add(reservationResult.Value);
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
