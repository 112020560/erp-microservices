using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Commands.AddPromotionAction;

public sealed record AddPromotionActionCommand(
    Guid PromotionId,
    PromotionActionType ActionType,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    Guid? TargetReferenceId,
    int? BuyQuantity,
    int? GetQuantity,
    Guid? BuyReferenceId,
    Guid? GetReferenceId) : ICommand<Guid>;
