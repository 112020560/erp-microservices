using Microsoft.EntityFrameworkCore;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;

namespace Retail.Infrastructure.Persistence.Repositories;

internal sealed class ChannelPriceListRepository(RetailDbContext context) : IChannelPriceListRepository
{
    public async Task<IReadOnlyList<ChannelPriceList>> GetByChannelAsync(
        SalesChannel channel,
        CancellationToken cancellationToken = default)
        => await context.ChannelPriceLists
            .Where(c => c.Channel == channel)
            .OrderByDescending(c => c.Priority)
            .ToListAsync(cancellationToken);

    public async Task<ChannelPriceList?> GetByChannelAndListAsync(
        SalesChannel channel,
        Guid priceListId,
        CancellationToken cancellationToken = default)
        => await context.ChannelPriceLists
            .FirstOrDefaultAsync(c => c.Channel == channel && c.PriceListId == priceListId, cancellationToken);

    public async Task AddAsync(ChannelPriceList assignment, CancellationToken cancellationToken = default)
        => await context.ChannelPriceLists.AddAsync(assignment, cancellationToken);

    public void Remove(ChannelPriceList assignment)
        => context.ChannelPriceLists.Remove(assignment);
}
