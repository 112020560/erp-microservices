using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class WarehouseRepository(InventoryDbContext context) : IWarehouseRepository
{
    public Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Warehouses
            .Include(w => w.Locations)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        context.Warehouses.AnyAsync(w => w.Code == code.Trim().ToUpperInvariant(), cancellationToken);

    public async Task<IReadOnlyList<Warehouse>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var list = await context.Warehouses
            .Include(w => w.Locations)
            .Where(w => w.IsActive)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public void Add(Warehouse warehouse) => context.Warehouses.Add(warehouse);

    public void Update(Warehouse warehouse) => context.Warehouses.Update(warehouse);
}
