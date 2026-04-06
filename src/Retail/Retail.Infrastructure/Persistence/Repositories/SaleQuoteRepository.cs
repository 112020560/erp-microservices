using Microsoft.EntityFrameworkCore;
using Retail.Domain.Sales;
using Retail.Domain.Sales.Abstractions;

namespace Retail.Infrastructure.Persistence.Repositories;

internal sealed class SaleQuoteRepository(RetailDbContext context) : ISaleQuoteRepository
{
    public async Task<SaleQuote?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.SaleQuotes.FindAsync([id], ct);

    public async Task<SaleQuote?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await context.SaleQuotes
            .Include(q => q.Lines)
            .Include(q => q.AppliedPromotions)
            .FirstOrDefaultAsync(q => q.Id == id, ct);

    public async Task<IReadOnlyList<SaleQuote>> GetAllAsync(
        SaleQuoteStatus? status, Guid? salesPersonId, Guid? customerId, CancellationToken ct = default)
    {
        var query = context.SaleQuotes
            .Include(q => q.Lines)
            .Include(q => q.AppliedPromotions)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(q => q.Status == status.Value);

        if (salesPersonId.HasValue)
            query = query.Where(q => q.SalesPersonId == salesPersonId.Value);

        if (customerId.HasValue)
            query = query.Where(q => q.CustomerId == customerId.Value);

        return await query.OrderByDescending(q => q.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SaleQuote>> GetExpiredOpenAsync(DateTimeOffset asOf, CancellationToken ct = default)
        => await context.SaleQuotes
            .Include(q => q.Lines)
            .Where(q => q.Status == SaleQuoteStatus.Open && q.ValidUntil < asOf)
            .ToListAsync(ct);

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await context.SaleQuotes.CountAsync(ct);

    public async Task AddAsync(SaleQuote quote, CancellationToken ct = default)
        => await context.SaleQuotes.AddAsync(quote, ct);

    public void Update(SaleQuote quote)
        => context.SaleQuotes.Update(quote);
}
