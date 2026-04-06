using SharedKernel;

namespace Retail.Domain.Sales;

public sealed class SaleInvoice
{
    private readonly List<PaymentLine> _payments = [];

    private SaleInvoice() { }

    public Guid Id { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid QuoteId { get; private set; }
    public Guid CashierId { get; private set; }
    public bool RequiresElectronicInvoice { get; private set; }
    public Guid? ElectronicDocumentId { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<PaymentLine> Payments => _payments.AsReadOnly();

    public static Result<SaleInvoice> Create(
        string invoiceNumber,
        Guid quoteId,
        Guid cashierId,
        bool requiresElectronicInvoice,
        decimal total,
        IReadOnlyList<PaymentLineRequest> payments)
    {
        if (payments.Count == 0)
            return Result.Failure<SaleInvoice>(SaleErrors.NoPayments);

        var paymentTotal = payments.Sum(p => p.Amount);
        if (Math.Abs(paymentTotal - total) > 0.01m)
            return Result.Failure<SaleInvoice>(SaleErrors.InvalidPaymentTotal);

        var invoice = new SaleInvoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            QuoteId = quoteId,
            CashierId = cashierId,
            RequiresElectronicInvoice = requiresElectronicInvoice,
            Total = total,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var p in payments)
            invoice._payments.Add(PaymentLine.Create(invoice.Id, p.Method, p.Amount, p.Reference));

        return Result.Success(invoice);
    }

    public void SetElectronicDocumentId(Guid documentId)
    {
        ElectronicDocumentId = documentId;
    }
}

public sealed record PaymentLineRequest(PaymentMethod Method, decimal Amount, string? Reference);
