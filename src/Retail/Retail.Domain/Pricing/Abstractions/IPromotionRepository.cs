namespace Retail.Domain.Pricing.Abstractions;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Promotion?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<Promotion?> GetByCouponCodeAsync(string couponCode, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> GetActiveAutomaticAsync(DateTimeOffset at, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> GetByIdsWithUsagesAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> GetAllAsync(bool? isActive = null, CancellationToken ct = default);
    Task AddAsync(Promotion promotion, CancellationToken ct = default);
    void Update(Promotion promotion);
}
