using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.PhysicalInventory;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class PhysicalCountRepository(InventoryDbContext context) : IPhysicalCountRepository
{
    public Task<PhysicalCount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.PhysicalCounts
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PhysicalCount>> GetByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var list = await context.PhysicalCounts
            .Include(c => c.Lines)
            .Where(c => c.WarehouseId == warehouseId)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public Task<int> GetNextSequenceAsync(CancellationToken cancellationToken = default) =>
        context.PhysicalCounts.CountAsync(cancellationToken);

    public void Add(PhysicalCount physicalCount) => context.PhysicalCounts.Add(physicalCount);

    public void Update(PhysicalCount physicalCount) => context.PhysicalCounts.Update(physicalCount);
}
