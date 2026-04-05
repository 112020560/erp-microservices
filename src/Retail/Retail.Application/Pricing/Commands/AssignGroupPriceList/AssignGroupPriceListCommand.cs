using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.AssignGroupPriceList;

public sealed record AssignGroupPriceListCommand(
    Guid GroupId,
    Guid PriceListId,
    int Priority,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo) : ICommand<Guid>;
