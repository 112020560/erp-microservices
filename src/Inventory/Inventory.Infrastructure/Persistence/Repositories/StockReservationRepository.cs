using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class StockReservationRepository(InventoryDbContext context) : IStockReservationRepository
{
    public Task<StockReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.StockReservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<StockReservation>> GetBySalesOrderAsync(Guid salesOrderId, CancellationToken cancellationToken = default)
    {
        var list = await context.StockReservations
            .Where(r => r.SalesOrderId == salesOrderId)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public Task<bool> ExistsByNumberAsync(string number, CancellationToken cancellationToken = default) =>
        context.StockReservations.AnyAsync(r => r.ReservationNumber == number, cancellationToken);

    public void Add(StockReservation reservation) => context.StockReservations.Add(reservation);

    public void Update(StockReservation reservation) => context.StockReservations.Update(reservation);
}
