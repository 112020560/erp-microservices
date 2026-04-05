using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.ActivatePromotion;

public sealed record ActivatePromotionCommand(Guid Id) : ICommand;
