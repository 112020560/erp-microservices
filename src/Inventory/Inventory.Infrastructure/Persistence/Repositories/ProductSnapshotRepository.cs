using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class ProductSnapshotRepository(InventoryDbContext context) : IProductSnapshotRepository
{
    public Task<ProductSnapshot?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
        context.ProductSnapshots.FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

    public async Task<IReadOnlyList<ProductSnapshot>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var list = await context.ProductSnapshots
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public void Add(ProductSnapshot snapshot) => context.ProductSnapshots.Add(snapshot);

    public void Update(ProductSnapshot snapshot) => context.ProductSnapshots.Update(snapshot);
}
