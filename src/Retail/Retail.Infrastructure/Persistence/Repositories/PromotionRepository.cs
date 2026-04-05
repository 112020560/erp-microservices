using Microsoft.EntityFrameworkCore;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;

namespace Retail.Infrastructure.Persistence.Repositories;

internal sealed class PromotionRepository(RetailDbContext context) : IPromotionRepository
{
    public async Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Promotions.FindAsync([id], ct);

    public async Task<Promotion?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await context.Promotions
            .Include(p => p.Conditions)
            .Include(p => p.Actions)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Promotion?> GetByCouponCodeAsync(string couponCode, CancellationToken ct = default)
        => await context.Promotions
            .Include(p => p.Conditions)
            .Include(p => p.Actions)
            .Include(p => p.Usages)
            .FirstOrDefaultAsync(p => p.CouponCode == couponCode, ct);

    public async Task<IReadOnlyList<Promotion>> GetActiveAutomaticAsync(DateTimeOffset at, CancellationToken ct = default)
        => await context.Promotions
            .Include(p => p.Conditions)
            .Include(p => p.Actions)
            .Where(p => p.IsActive
                && p.CouponCode == null
                && (p.ValidFrom == null || p.ValidFrom <= at)
                && (p.ValidTo == null || p.ValidTo >= at))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Promotion>> GetAllAsync(bool? isActive = null, CancellationToken ct = default)
    {
        var query = context.Promotions
            .Include(p => p.Conditions)
            .Include(p => p.Actions)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        return await query.OrderByDescending(p => p.Priority).ToListAsync(ct);
    }

    public async Task AddAsync(Promotion promotion, CancellationToken ct = default)
        => await context.Promotions.AddAsync(promotion, ct);

    public void Update(Promotion promotion)
        => context.Promotions.Update(promotion);
}
