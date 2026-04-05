using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;

namespace Retail.Application.Pricing.Commands.AddPriceListItem;

public sealed record AddPriceListItemCommand(
    Guid PriceListId,
    PriceItemType ItemType,
    Guid? ReferenceId,
    decimal MinQuantity,
    decimal? MaxQuantity,
    decimal Price,
    decimal DiscountPercentage,
    decimal? MinPrice,
    bool PriceIncludesTax) : ICommand<Guid>;
