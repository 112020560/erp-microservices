using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.AddOrderDiscount;

public sealed record AddOrderDiscountCommand(
    Guid PriceListId,
    decimal? MinOrderTotal,
    decimal? MinOrderQuantity,
    decimal DiscountPercentage,
    decimal? DiscountAmount,
    decimal? MaxDiscountAmount,
    int Priority) : ICommand<Guid>;
