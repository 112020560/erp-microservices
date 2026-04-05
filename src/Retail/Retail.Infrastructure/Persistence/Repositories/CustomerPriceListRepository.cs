using Microsoft.EntityFrameworkCore;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;

namespace Retail.Infrastructure.Persistence.Repositories;

internal sealed class CustomerPriceListRepository(RetailDbContext context) : ICustomerPriceListRepository
{
    public async Task<IReadOnlyList<CustomerPriceList>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
        => await context.CustomerPriceLists
            .Where(c => c.CustomerId == customerId)
            .ToListAsync(cancellationToken);

    public async Task<CustomerPriceList?> GetActiveByCustomerAsync(
        Guid customerId,
        DateTimeOffset at,
        CancellationToken cancellationToken = default)
        => await context.CustomerPriceLists
            .Where(c => c.CustomerId == customerId &&
                        (!c.ValidFrom.HasValue || c.ValidFrom <= at) &&
                        (!c.ValidTo.HasValue || c.ValidTo >= at))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(CustomerPriceList assignment, CancellationToken cancellationToken = default)
        => await context.CustomerPriceLists.AddAsync(assignment, cancellationToken);

    public void Remove(CustomerPriceList assignment)
        => context.CustomerPriceLists.Remove(assignment);
}
