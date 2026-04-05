using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.DeactivatePromotion;

public sealed record DeactivatePromotionCommand(Guid Id) : ICommand;
