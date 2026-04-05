using Microsoft.EntityFrameworkCore;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;

namespace Retail.Infrastructure.Persistence.Repositories;

internal sealed class ScheduledPriceChangeRepository(RetailDbContext context) : IScheduledPriceChangeRepository
{
    public async Task<ScheduledPriceChange?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.ScheduledPriceChanges.FindAsync([id], ct);

    public async Task<IReadOnlyList<ScheduledPriceChange>> GetPendingDueAsync(DateTimeOffset asOf, CancellationToken ct = default)
        => await context.ScheduledPriceChanges
            .Where(c => c.Status == ScheduledPriceChangeStatus.Pending && c.EffectiveAt <= asOf)
            .OrderBy(c => c.EffectiveAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ScheduledPriceChange>> GetByPriceListAsync(Guid priceListId, CancellationToken ct = default)
        => await context.ScheduledPriceChanges
            .Where(c => c.PriceListId == priceListId)
            .OrderBy(c => c.EffectiveAt)
            .ToListAsync(ct);

    public async Task AddAsync(ScheduledPriceChange change, CancellationToken ct = default)
        => await context.ScheduledPriceChanges.AddAsync(change, ct);

    public void Update(ScheduledPriceChange change)
        => context.ScheduledPriceChanges.Update(change);
}
