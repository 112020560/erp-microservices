namespace Retail.Domain.Sales;

public sealed class SaleQuoteLine
{
    private SaleQuoteLine() { }

    public Guid Id { get; private set; }
    public Guid QuoteId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal LineTotal { get; private set; }
    public string? PriceListName { get; private set; }
    public string? ResolutionSource { get; private set; }

    internal static SaleQuoteLine Create(
        Guid quoteId, Guid productId, string sku, string productName, Guid? categoryId,
        decimal quantity, decimal unitPrice, decimal discountPercentage, decimal lineTotal,
        string? priceListName, string? resolutionSource)
    {
        return new SaleQuoteLine
        {
            Id = Guid.NewGuid(),
            QuoteId = quoteId,
            ProductId = productId,
            Sku = sku,
            ProductName = productName,
            CategoryId = categoryId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercentage = discountPercentage,
            LineTotal = lineTotal,
            PriceListName = priceListName,
            ResolutionSource = resolutionSource
        };
    }
}
