using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Queries.ResolvePrice;

public sealed record ResolvePriceQuery(
    Guid? CustomerId,
    SalesChannel Channel,
    IReadOnlyList<PriceRequestItem> Items,
    string Currency) : IQuery<IReadOnlyList<ResolvedPriceResponse>>;

public sealed record PriceRequestItem(
    Guid ProductId,
    Guid CategoryId,
    decimal Quantity);
