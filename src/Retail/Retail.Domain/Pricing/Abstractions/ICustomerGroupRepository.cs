namespace Retail.Domain.Pricing.Abstractions;

public interface ICustomerGroupRepository
{
    Task<CustomerGroup?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CustomerGroup?> GetByIdWithMembersAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerGroup>> GetAllAsync(bool? isActive = null, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerGroupPriceList>> GetActiveGroupPriceListsForCustomerAsync(Guid customerId, DateTimeOffset at, CancellationToken ct = default);
    Task AddAsync(CustomerGroup group, CancellationToken ct = default);
    Task AddGroupPriceListAsync(CustomerGroupPriceList assignment, CancellationToken ct = default);
    void Update(CustomerGroup group);
}
