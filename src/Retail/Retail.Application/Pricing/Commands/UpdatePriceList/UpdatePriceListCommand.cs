using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Commands.UpdatePriceList;

public sealed record UpdatePriceListCommand(
    Guid Id,
    string Name,
    int Priority,
    RoundingRule RoundingRule,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo) : ICommand;
