using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.RemovePriceListItem;

public sealed record RemovePriceListItemCommand(Guid PriceListId, Guid ItemId) : ICommand;
