using Retail.Domain.Pricing;
using SharedKernel;

namespace Retail.Domain.Sales;

public sealed class SaleQuote
{
    private readonly List<SaleQuoteLine> _lines = [];
    private readonly List<AppliedPromotion> _appliedPromotions = [];

    private SaleQuote() { }

    public Guid Id { get; private set; }
    public string QuoteNumber { get; private set; } = string.Empty;
    public Guid SalesPersonId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public Guid WarehouseId { get; private set; }
    public SalesChannel Channel { get; private set; }
    public SaleQuoteStatus Status { get; private set; }
    public DateTimeOffset ValidUntil { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal VolumeDiscountAmount { get; private set; }
    public decimal PromotionDiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<SaleQuoteLine> Lines => _lines.AsReadOnly();
    public IReadOnlyList<AppliedPromotion> AppliedPromotions => _appliedPromotions.AsReadOnly();

    public static Result<SaleQuote> Create(
        string quoteNumber,
        Guid salesPersonId,
        Guid? customerId,
        string customerName,
        Guid warehouseId,
        SalesChannel channel,
        DateTimeOffset validUntil,
        string currency,
        string? notes,
        decimal subtotal,
        decimal volumeDiscountAmount,
        decimal promotionDiscountAmount,
        decimal taxAmount,
        decimal total,
        IReadOnlyList<SaleQuoteLineRequest> lines,
        IReadOnlyList<AppliedPromotionRequest> appliedPromotions)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            customerName = "Cliente Contado";

        if (validUntil <= DateTimeOffset.UtcNow)
            return Result.Failure<SaleQuote>(SaleErrors.InvalidValidUntil);

        if (lines.Count == 0)
            return Result.Failure<SaleQuote>(SaleErrors.NoLines);

        var quote = new SaleQuote
        {
            Id = Guid.NewGuid(),
            QuoteNumber = quoteNumber,
            SalesPersonId = salesPersonId,
            CustomerId = customerId,
            CustomerName = customerName.Trim(),
            WarehouseId = warehouseId,
            Channel = channel,
            Status = SaleQuoteStatus.Draft,
            ValidUntil = validUntil,
            Currency = currency.ToUpperInvariant(),
            Notes = notes?.Trim(),
            Subtotal = subtotal,
            VolumeDiscountAmount = volumeDiscountAmount,
            PromotionDiscountAmount = promotionDiscountAmount,
            TaxAmount = taxAmount,
            Total = total,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        foreach (var line in lines)
        {
            if (line.Quantity <= 0) return Result.Failure<SaleQuote>(SaleErrors.InvalidQuantity);
            if (line.UnitPrice < 0) return Result.Failure<SaleQuote>(SaleErrors.InvalidPrice);

            quote._lines.Add(SaleQuoteLine.Create(
                quote.Id, line.ProductId, line.Sku, line.ProductName, line.CategoryId,
                line.Quantity, line.UnitPrice, line.DiscountPercentage, line.LineTotal,
                line.PriceListName, line.ResolutionSource));
        }

        foreach (var promo in appliedPromotions)
            quote._appliedPromotions.Add(AppliedPromotion.Create(quote.Id, promo.PromotionId, promo.PromotionName, promo.DiscountAmount));

        return Result.Success(quote);
    }

    public Result Confirm()
    {
        if (Status != SaleQuoteStatus.Draft)
            return Result.Failure(SaleErrors.QuoteNotDraft);
        Status = SaleQuoteStatus.Open;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is SaleQuoteStatus.Invoiced or SaleQuoteStatus.Expired)
            return Result.Failure(SaleErrors.QuoteCannotBeCancelled);
        if (Status == SaleQuoteStatus.Cancelled)
            return Result.Failure(SaleErrors.QuoteCancelled);
        Status = SaleQuoteStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result MarkInvoiced()
    {
        if (Status != SaleQuoteStatus.Open)
            return Result.Failure(SaleErrors.QuoteNotOpen);
        Status = SaleQuoteStatus.Invoiced;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Expire()
    {
        if (Status != SaleQuoteStatus.Open)
            return Result.Failure(SaleErrors.QuoteNotOpen);
        Status = SaleQuoteStatus.Expired;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public bool IsExpired() => ValidUntil < DateTimeOffset.UtcNow;
}

// Request records used only for construction (not stored)
public sealed record SaleQuoteLineRequest(
    Guid ProductId, string Sku, string ProductName, Guid? CategoryId,
    decimal Quantity, decimal UnitPrice, decimal DiscountPercentage, decimal LineTotal,
    string? PriceListName, string? ResolutionSource);

public sealed record AppliedPromotionRequest(Guid PromotionId, string PromotionName, decimal DiscountAmount);
