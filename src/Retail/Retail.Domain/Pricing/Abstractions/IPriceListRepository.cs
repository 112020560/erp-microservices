namespace Retail.Domain.Pricing.Abstractions;

public interface IPriceListRepository
{
    Task<PriceList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PriceList?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PriceList?> GetByIdWithItemsAndDiscountsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceList>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default);
    void Update(PriceList priceList);
}
