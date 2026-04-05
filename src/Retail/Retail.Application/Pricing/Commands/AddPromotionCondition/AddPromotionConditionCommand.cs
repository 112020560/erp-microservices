using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Commands.AddPromotionCondition;

public sealed record AddPromotionConditionCommand(
    Guid PromotionId,
    PromotionConditionType ConditionType,
    decimal? DecimalValue,
    Guid? ReferenceId,
    int? IntValue) : ICommand<Guid>;
