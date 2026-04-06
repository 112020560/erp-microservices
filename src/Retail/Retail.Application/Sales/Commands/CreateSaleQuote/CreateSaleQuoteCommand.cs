using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Sales;

namespace Retail.Application.Sales.Commands.CreateSaleQuote;

public sealed record CreateSaleQuoteCommand(
    Guid SalesPersonId,
    Guid? CustomerId,
    string CustomerName,
    Guid WarehouseId,
    SalesChannel Channel,
    DateTimeOffset ValidUntil,
    string Currency,
    string? Notes,
    decimal Subtotal,
    decimal VolumeDiscountAmount,
    decimal PromotionDiscountAmount,
    decimal TaxAmount,
    decimal Total,
    IReadOnlyList<CreateSaleQuoteLineDto> Lines,
    IReadOnlyList<CreateAppliedPromotionDto> AppliedPromotions) : ICommand<Guid>;

public sealed record CreateSaleQuoteLineDto(
    Guid ProductId, string Sku, string ProductName, Guid? CategoryId,
    decimal Quantity, decimal UnitPrice, decimal DiscountPercentage, decimal LineTotal,
    string? PriceListName, string? ResolutionSource);

public sealed record CreateAppliedPromotionDto(Guid PromotionId, string PromotionName, decimal DiscountAmount);
