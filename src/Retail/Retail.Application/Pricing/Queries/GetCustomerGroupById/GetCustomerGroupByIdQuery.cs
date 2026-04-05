using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Queries.GetCustomerGroupById;

public sealed record GetCustomerGroupByIdQuery(Guid Id) : IQuery<CustomerGroupDetailResponse>;
