using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Movements;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryMovementRepository(InventoryDbContext context) : IInventoryMovementRepository
{
    public Task<InventoryMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.InventoryMovements
            .Include(m => m.Lines)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<InventoryMovement>> GetByWarehouseAsync(
        Guid warehouseId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var query = context.InventoryMovements
            .Include(m => m.Lines)
            .AsQueryable();

        if (warehouseId != Guid.Empty)
            query = query.Where(m => m.WarehouseId == warehouseId || m.DestinationWarehouseId == warehouseId);

        if (from.HasValue)
            query = query.Where(m => m.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.Date <= to.Value);

        var list = await query.OrderByDescending(m => m.Date).ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public Task<int> GetNextSequenceAsync(CancellationToken cancellationToken = default) =>
        context.InventoryMovements.CountAsync(cancellationToken);

    public void Add(InventoryMovement movement) => context.InventoryMovements.Add(movement);

    public void Update(InventoryMovement movement) => context.InventoryMovements.Update(movement);
}
