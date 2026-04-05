using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.AssignCustomerPriceList;

public sealed record AssignCustomerPriceListCommand(
    Guid CustomerId,
    Guid PriceListId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo) : ICommand<Guid>;
