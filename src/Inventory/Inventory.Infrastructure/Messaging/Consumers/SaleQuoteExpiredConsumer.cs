using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Stock;
using MassTransit;
using SharedKernel.Contracts.Sales;

namespace Inventory.Infrastructure.Messaging.Consumers;

public sealed class SaleQuoteExpiredConsumer(
    IStockReservationRepository reservationRepository,
    IStockEntryRepository stockEntryRepository,
    IUnitOfWork unitOfWork) : IConsumer<SaleQuoteExpiredEvent>
{
    public async Task Consume(ConsumeContext<SaleQuoteExpiredEvent> context)
    {
        var msg = context.Message;

        var reservations = await reservationRepository.GetBySalesOrderAsync(msg.QuoteId, context.CancellationToken);

        foreach (var reservation in reservations.Where(r => r.Status == ReservationStatus.Active))
        {
            // Expire reservation record
            reservation.Expire();
            reservationRepository.Update(reservation);

            // Release quantity on StockEntry
            var stockEntries = await stockEntryRepository.GetByProductAsync(reservation.ProductId, context.CancellationToken);
            var stockEntry = stockEntries.FirstOrDefault(se =>
                se.WarehouseId == reservation.WarehouseId &&
                se.LocationId == reservation.LocationId);

            if (stockEntry is not null)
            {
                stockEntry.ReleaseReservation(reservation.ReservedQuantity);
                stockEntryRepository.Update(stockEntry);
            }
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
