using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Queries.GetPromotionById;

public sealed record GetPromotionByIdQuery(Guid Id) : IQuery<PromotionDetailResponse>;
