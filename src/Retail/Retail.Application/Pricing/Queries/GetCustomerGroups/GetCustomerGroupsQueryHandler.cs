using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.GetCustomerGroups;

internal sealed class GetCustomerGroupsQueryHandler(ICustomerGroupRepository customerGroupRepository)
    : IQueryHandler<GetCustomerGroupsQuery, IReadOnlyList<CustomerGroupSummaryResponse>>
{
    public async Task<Result<IReadOnlyList<CustomerGroupSummaryResponse>>> Handle(
        GetCustomerGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var groups = await customerGroupRepository.GetAllAsync(request.IsActive, cancellationToken);

        var response = groups
            .Select(g => new CustomerGroupSummaryResponse(
                g.Id,
                g.Name,
                g.Description,
                g.IsActive,
                g.Members.Count,
                g.CreatedAt))
            .ToList()
            .AsReadOnly() as IReadOnlyList<CustomerGroupSummaryResponse>;

        return Result.Success(response!);
    }
}
