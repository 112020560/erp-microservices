using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Queries.GetCustomerGroups;

public sealed record GetCustomerGroupsQuery(bool? IsActive) : IQuery<IReadOnlyList<CustomerGroupSummaryResponse>>;
