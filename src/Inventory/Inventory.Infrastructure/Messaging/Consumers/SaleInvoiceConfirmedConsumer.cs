using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Stock;
using MassTransit;
using SharedKernel.Contracts.Sales;

namespace Inventory.Infrastructure.Messaging.Consumers;

public sealed class SaleInvoiceConfirmedConsumer(
    IStockReservationRepository reservationRepository,
    IStockEntryRepository stockEntryRepository,
    IUnitOfWork unitOfWork) : IConsumer<SaleInvoiceConfirmedEvent>
{
    public async Task Consume(ConsumeContext<SaleInvoiceConfirmedEvent> context)
    {
        var msg = context.Message;

        var reservations = await reservationRepository.GetBySalesOrderAsync(msg.QuoteId, context.CancellationToken);

        // Idempotency: if no active reservations remain, skip
        if (reservations.All(r => r.Status != ReservationStatus.Active)) return;

        foreach (var reservation in reservations.Where(r => r.Status == ReservationStatus.Active))
        {
            // Mark reservation as released (consumed by sale)
            reservation.Release();
            reservationRepository.Update(reservation);

            // Deduct from StockEntry: release reservation count then issue the stock
            var stockEntries = await stockEntryRepository.GetByProductAsync(reservation.ProductId, context.CancellationToken);
            var stockEntry = stockEntries.FirstOrDefault(se =>
                se.WarehouseId == reservation.WarehouseId &&
                se.LocationId == reservation.LocationId);

            if (stockEntry is not null)
            {
                // Release reservation count first so QuantityAvailable is correct for IssueStock
                stockEntry.ReleaseReservation(reservation.ReservedQuantity);
                stockEntry.IssueStock(reservation.ReservedQuantity);
                stockEntryRepository.Update(stockEntry);
            }
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
