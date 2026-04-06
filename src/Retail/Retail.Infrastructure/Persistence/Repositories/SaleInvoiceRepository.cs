using Microsoft.EntityFrameworkCore;
using Retail.Domain.Sales;
using Retail.Domain.Sales.Abstractions;

namespace Retail.Infrastructure.Persistence.Repositories;

internal sealed class SaleInvoiceRepository(RetailDbContext context) : ISaleInvoiceRepository
{
    public async Task<SaleInvoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.SaleInvoices.FindAsync([id], ct);

    public async Task<SaleInvoice?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await context.SaleInvoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<SaleInvoice>> GetAllAsync(CancellationToken ct = default)
        => await context.SaleInvoices
            .Include(i => i.Payments)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await context.SaleInvoices.CountAsync(ct);

    public async Task<SaleInvoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
        => await context.SaleInvoices
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, ct);

    public async Task AddAsync(SaleInvoice invoice, CancellationToken ct = default)
        => await context.SaleInvoices.AddAsync(invoice, ct);

    public void Update(SaleInvoice invoice)
        => context.SaleInvoices.Update(invoice);
}
