using Microsoft.EntityFrameworkCore;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;

namespace Retail.Infrastructure.Persistence.Repositories;

internal sealed class CustomerGroupRepository(RetailDbContext context) : ICustomerGroupRepository
{
    public async Task<CustomerGroup?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.CustomerGroups.FindAsync([id], ct);

    public async Task<CustomerGroup?> GetByIdWithMembersAsync(Guid id, CancellationToken ct = default)
        => await context.CustomerGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<IReadOnlyList<CustomerGroup>> GetAllAsync(bool? isActive = null, CancellationToken ct = default)
    {
        var query = context.CustomerGroups
            .Include(g => g.Members)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(g => g.IsActive == isActive.Value);

        return await query.OrderBy(g => g.Name).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CustomerGroupPriceList>> GetActiveGroupPriceListsForCustomerAsync(
        Guid customerId,
        DateTimeOffset at,
        CancellationToken ct = default)
    {
        var groupIds = await context.CustomerGroupMembers
            .Where(m => m.CustomerId == customerId)
            .Select(m => m.GroupId)
            .ToListAsync(ct);

        return await context.CustomerGroupPriceLists
            .Where(gpl => groupIds.Contains(gpl.GroupId) &&
                          (gpl.ValidFrom == null || gpl.ValidFrom <= at) &&
                          (gpl.ValidTo == null || gpl.ValidTo >= at))
            .OrderByDescending(gpl => gpl.Priority)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CustomerGroup group, CancellationToken ct = default)
        => await context.CustomerGroups.AddAsync(group, ct);

    public async Task AddGroupPriceListAsync(CustomerGroupPriceList assignment, CancellationToken ct = default)
        => await context.CustomerGroupPriceLists.AddAsync(assignment, ct);

    public void Update(CustomerGroup group)
        => context.CustomerGroups.Update(group);
}
