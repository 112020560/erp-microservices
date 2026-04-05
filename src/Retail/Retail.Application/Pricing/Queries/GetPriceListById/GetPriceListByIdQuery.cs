using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Queries.GetPriceListById;

public sealed record GetPriceListByIdQuery(Guid Id) : IQuery<PriceListDetailResponse>;
