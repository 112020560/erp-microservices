using Retail.Domain.Pricing.Abstractions;

namespace Retail.Infrastructure.Persistence.Repositories;

internal sealed class UnitOfWork(RetailDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
