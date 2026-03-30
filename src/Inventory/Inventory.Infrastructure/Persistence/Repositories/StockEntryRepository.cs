using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class StockEntryRepository(InventoryDbContext context) : IStockEntryRepository
{
    public Task<StockEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.StockEntries.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<StockEntry?> GetAsync(Guid productId, Guid warehouseId, Guid locationId, Guid? lotId, CancellationToken cancellationToken = default) =>
        context.StockEntries.FirstOrDefaultAsync(
            s => s.ProductId == productId &&
                 s.WarehouseId == warehouseId &&
                 s.LocationId == locationId &&
                 s.LotId == lotId,
            cancellationToken);

    public async Task<IReadOnlyList<StockEntry>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var list = await context.StockEntries
            .Where(s => s.ProductId == productId)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public async Task<IReadOnlyList<StockEntry>> GetByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var query = context.StockEntries.AsQueryable();
        if (warehouseId != Guid.Empty)
            query = query.Where(s => s.WarehouseId == warehouseId);
        var list = await query.ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public async Task<IReadOnlyList<StockEntry>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var list = await context.StockEntries
            .Where(s => s.MinimumStock > 0 && s.QuantityOnHand <= s.MinimumStock)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public void Add(StockEntry stockEntry) => context.StockEntries.Add(stockEntry);

    public void Update(StockEntry stockEntry) => context.StockEntries.Update(stockEntry);
}
