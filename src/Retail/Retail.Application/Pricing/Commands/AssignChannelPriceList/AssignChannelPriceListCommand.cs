using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Commands.AssignChannelPriceList;

public sealed record AssignChannelPriceListCommand(
    SalesChannel Channel,
    Guid PriceListId,
    int Priority) : ICommand<Guid>;
