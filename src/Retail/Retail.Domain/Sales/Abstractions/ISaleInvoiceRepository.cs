namespace Retail.Domain.Sales.Abstractions;

public interface ISaleInvoiceRepository
{
    Task<SaleInvoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SaleInvoice?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SaleInvoice>> GetAllAsync(CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<SaleInvoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default);
    Task AddAsync(SaleInvoice invoice, CancellationToken ct = default);
    void Update(SaleInvoice invoice);
}
