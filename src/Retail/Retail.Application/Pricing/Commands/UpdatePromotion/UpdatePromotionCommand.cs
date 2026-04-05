using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.UpdatePromotion;

public sealed record UpdatePromotionCommand(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    int? MaxUses,
    int? MaxUsesPerCustomer,
    int Priority,
    bool CanStackWithOthers) : ICommand;
