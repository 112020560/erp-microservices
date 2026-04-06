namespace Retail.Domain.Sales;

public sealed class PaymentLine
{
    private PaymentLine() { }

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }
    public string? Reference { get; private set; }

    internal static PaymentLine Create(Guid invoiceId, PaymentMethod method, decimal amount, string? reference)
        => new() { Id = Guid.NewGuid(), InvoiceId = invoiceId, Method = method, Amount = amount, Reference = reference };
}
