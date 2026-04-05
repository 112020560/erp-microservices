using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.GetCustomerGroupById;

internal sealed class GetCustomerGroupByIdQueryHandler(ICustomerGroupRepository customerGroupRepository)
    : IQueryHandler<GetCustomerGroupByIdQuery, CustomerGroupDetailResponse>
{
    public async Task<Result<CustomerGroupDetailResponse>> Handle(
        GetCustomerGroupByIdQuery request,
        CancellationToken cancellationToken)
    {
        var group = await customerGroupRepository.GetByIdWithMembersAsync(request.Id, cancellationToken);
        if (group is null) return Result.Failure<CustomerGroupDetailResponse>(PriceListErrors.CustomerGroupNotFound);

        var members = group.Members
            .Select(m => new CustomerGroupMemberResponse(m.Id, m.CustomerId, m.AddedAt))
            .ToList()
            .AsReadOnly() as IReadOnlyList<CustomerGroupMemberResponse>;

        var response = new CustomerGroupDetailResponse(
            group.Id,
            group.Name,
            group.Description,
            group.IsActive,
            members!,
            group.CreatedAt,
            group.UpdatedAt);

        return Result.Success(response);
    }
}
