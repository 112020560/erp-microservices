namespace SharedKernel.Contracts.Sales;

public record SaleInvoiceConfirmedEvent
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid QuoteId { get; init; }
    public Guid WarehouseId { get; init; }
    public IReadOnlyList<SaleInvoiceLineContract> Lines { get; init; } = [];
    public bool RequiresElectronicInvoice { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTimeOffset ConfirmedAt { get; init; }
}

public record SaleInvoiceLineContract(
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);
