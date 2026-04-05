namespace Retail.Application.Pricing.Queries.GetCustomerGroupById;

public sealed record CustomerGroupMemberResponse(Guid MemberId, Guid CustomerId, DateTimeOffset AddedAt);

public sealed record CustomerGroupDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<CustomerGroupMemberResponse> Members,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
