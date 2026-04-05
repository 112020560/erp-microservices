using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.CreatePromotion;

public sealed record CreatePromotionCommand(
    string Name,
    string? Description,
    string? CouponCode,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    int? MaxUses,
    int? MaxUsesPerCustomer,
    int Priority,
    bool CanStackWithOthers) : ICommand<Guid>;
