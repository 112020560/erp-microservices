using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Commands.CreatePriceList;

public sealed record CreatePriceListCommand(
    string Name,
    string Currency,
    int Priority,
    RoundingRule RoundingRule,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo) : ICommand<Guid>;
