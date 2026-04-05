namespace Retail.Domain.Pricing.Abstractions;

public interface ICustomerPriceListRepository
{
    Task<IReadOnlyList<CustomerPriceList>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerPriceList?> GetActiveByCustomerAsync(Guid customerId, DateTimeOffset at, CancellationToken cancellationToken = default);
    Task AddAsync(CustomerPriceList assignment, CancellationToken cancellationToken = default);
    void Remove(CustomerPriceList assignment);
}
