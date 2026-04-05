using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Repositorios;

public class ElectronicDocumentLogRepository : IElectronicDocumentLogRepository
{
    private readonly AppDbContext _dbContext;

    public ElectronicDocumentLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddLogAsync(ElectronicDocumentLog log, CancellationToken cancellationToken)
    {
        log.CreatedAt = DateTime.UtcNow;
        await _dbContext.ElectronicDocumentLogs.AddAsync(log, cancellationToken);
    }

    public async Task<List<ElectronicDocumentLog>> GetByInvoiceIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ElectronicDocumentLogs
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
