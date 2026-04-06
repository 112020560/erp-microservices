namespace Retail.Domain.Sales.Abstractions;

public interface ISaleNumberGenerator
{
    Task<string> NextQuoteNumberAsync(CancellationToken ct = default);
    Task<string> NextInvoiceNumberAsync(CancellationToken ct = default);
}
