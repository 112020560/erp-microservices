using Inventory.Domain.Abstractions.Persistence;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class UnitOfWork(InventoryDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
