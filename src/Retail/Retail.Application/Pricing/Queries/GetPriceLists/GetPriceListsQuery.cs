using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Queries.GetPriceLists;

public sealed record GetPriceListsQuery(bool? IsActive) : IQuery<IReadOnlyList<PriceListSummaryResponse>>;
