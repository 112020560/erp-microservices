namespace Retail.Domain.Pricing.Abstractions;

public interface IScheduledPriceChangeRepository
{
    Task<ScheduledPriceChange?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ScheduledPriceChange>> GetPendingDueAsync(DateTimeOffset asOf, CancellationToken ct = default);
    Task<IReadOnlyList<ScheduledPriceChange>> GetByPriceListAsync(Guid priceListId, CancellationToken ct = default);
    Task AddAsync(ScheduledPriceChange change, CancellationToken ct = default);
    void Update(ScheduledPriceChange change);
}
