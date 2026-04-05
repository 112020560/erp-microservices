namespace Retail.Application.Pricing.Queries.GetCustomerGroups;

public sealed record CustomerGroupSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int MemberCount,
    DateTimeOffset CreatedAt);
