namespace Retail.Domain.Sales.Abstractions;

public interface ISaleQuoteRepository
{
    Task<SaleQuote?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SaleQuote?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SaleQuote>> GetAllAsync(SaleQuoteStatus? status, Guid? salesPersonId, Guid? customerId, CancellationToken ct = default);
    Task<IReadOnlyList<SaleQuote>> GetExpiredOpenAsync(DateTimeOffset asOf, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task AddAsync(SaleQuote quote, CancellationToken ct = default);
    void Update(SaleQuote quote);
}
