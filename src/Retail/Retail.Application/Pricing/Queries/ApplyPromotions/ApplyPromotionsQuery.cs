using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Queries.ApplyPromotions;

public sealed record ApplyPromotionsQuery(
    Guid? CustomerId,
    Guid[] CustomerGroupIds,
    string Channel,
    CartLineItem[] Items,
    decimal Subtotal,
    decimal TotalQuantity,
    string? CouponCode) : IQuery<PromotionsApplicationResult>;

public sealed record CartLineItem(
    Guid ProductId,
    Guid? CategoryId,
    decimal Quantity,
    decimal LineTotal);
