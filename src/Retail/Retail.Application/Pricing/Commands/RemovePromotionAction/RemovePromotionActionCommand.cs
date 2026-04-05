using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.RemovePromotionAction;

public sealed record RemovePromotionActionCommand(Guid PromotionId, Guid ActionId) : ICommand;
