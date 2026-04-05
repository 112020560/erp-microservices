using Microsoft.EntityFrameworkCore;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;

namespace Retail.Infrastructure.Persistence.Repositories;

internal sealed class PriceListRepository(RetailDbContext context) : IPriceListRepository
{
    public async Task<PriceList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.PriceLists.FindAsync([id], cancellationToken);

    public async Task<PriceList?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.PriceLists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PriceList?> GetByIdWithItemsAndDiscountsAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.PriceLists
            .Include(p => p.Items)
            .Include(p => p.OrderDiscounts)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PriceList>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = context.PriceLists
            .Include(p => p.Items)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        return await query.OrderByDescending(p => p.Priority).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default)
        => await context.PriceLists.AddAsync(priceList, cancellationToken);

    public void Update(PriceList priceList)
        => context.PriceLists.Update(priceList);
}
