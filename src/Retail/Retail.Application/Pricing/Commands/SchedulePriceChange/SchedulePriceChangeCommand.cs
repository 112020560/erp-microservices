using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.SchedulePriceChange;

public sealed record SchedulePriceChangeCommand(
    Guid PriceListId,
    Guid ItemId,
    decimal NewPrice,
    decimal? NewDiscountPercentage,
    decimal? NewMinPrice,
    DateTimeOffset EffectiveAt) : ICommand<Guid>;
