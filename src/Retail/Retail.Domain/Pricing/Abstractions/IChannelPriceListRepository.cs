namespace Retail.Domain.Pricing.Abstractions;

public interface IChannelPriceListRepository
{
    Task<IReadOnlyList<ChannelPriceList>> GetByChannelAsync(SalesChannel channel, CancellationToken cancellationToken = default);
    Task<ChannelPriceList?> GetByChannelAndListAsync(SalesChannel channel, Guid priceListId, CancellationToken cancellationToken = default);
    Task AddAsync(ChannelPriceList assignment, CancellationToken cancellationToken = default);
    void Remove(ChannelPriceList assignment);
}
