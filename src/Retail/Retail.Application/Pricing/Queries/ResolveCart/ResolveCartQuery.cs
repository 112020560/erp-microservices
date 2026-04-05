using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.ResolveCart;

public sealed record ResolveCartQuery(
    Guid? CustomerId,
    SalesChannel Channel,
    string Currency,
    IReadOnlyList<CartItemRequest> Items) : IQuery<CartResolutionResponse>;

public sealed record CartItemRequest(
    Guid ProductId,
    Guid? CategoryId,
    decimal Quantity);
