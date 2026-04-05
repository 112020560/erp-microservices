using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.RemovePromotionCondition;

public sealed record RemovePromotionConditionCommand(Guid PromotionId, Guid ConditionId) : ICommand;
