using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Queries.GetPromotions;

public sealed record GetPromotionsQuery(bool? IsActive) : IQuery<IReadOnlyList<PromotionSummaryResponse>>;
