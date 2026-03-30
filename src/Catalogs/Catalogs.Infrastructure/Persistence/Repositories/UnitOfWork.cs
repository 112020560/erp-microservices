using Catalogs.Domain.Abstractions.Persistence;

namespace Catalogs.Infrastructure.Persistence.Repositories;

internal sealed class UnitOfWork(CatalogsDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
