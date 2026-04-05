using Retail.Application.Abstractions.Messaging;

namespace Retail.Application.Pricing.Commands.RemoveOrderDiscount;

public sealed record RemoveOrderDiscountCommand(Guid PriceListId, Guid DiscountId) : ICommand;
