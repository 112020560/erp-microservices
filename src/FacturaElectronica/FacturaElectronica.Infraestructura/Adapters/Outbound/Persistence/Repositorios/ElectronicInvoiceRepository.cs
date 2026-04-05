using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Repositorios;

public class ElectronicInvoiceRepository : IElectronicInvoiceRepository
{
    private readonly AppDbContext _dbContext;

    public ElectronicInvoiceRepository(AppDbContext feDbContext)
    {
        _dbContext = feDbContext;
    }

    public async Task AddAsync(ElectronicInvoice electronicInvoice, CancellationToken cancellationToken = default)
    {
        await _dbContext.ElectronicDocuments.AddAsync(electronicInvoice, cancellationToken);
    }

    public async Task UpdateAsync(ElectronicInvoice electronicInvoice, CancellationToken cancellationToken = default)
    {
        await _dbContext.ElectronicDocuments
            .Where(x => x.Id == electronicInvoice.Id && x.TenantId == electronicInvoice.TenantId)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(p => p.Status, electronicInvoice.Status)
                .SetProperty(p => p.StatusDetail, electronicInvoice.StatusDetail)
                .SetProperty(p => p.XmlReceptorPath, electronicInvoice.XmlReceptorPath)
                .SetProperty(p => p.XmlRespuestaPath, electronicInvoice.XmlRespuestaPath)
                .SetProperty(p => p.ResponseMessage, electronicInvoice.ResponseMessage)
                .SetProperty(p => p.Error, electronicInvoice.Error)
                .SetProperty(p => p.FechaRespuesta, electronicInvoice.FechaRespuesta)
                .SetProperty(p => p.NotificacionEnviada, electronicInvoice.NotificacionEnviada)
                .SetProperty(p => p.FechaNotificacion, electronicInvoice.FechaNotificacion)
                .SetProperty(p => p.UpdatedAt, electronicInvoice.UpdatedAt),
            cancellationToken: cancellationToken);
    }

    public async Task<ElectronicInvoice?> GetByClaveAsync(string clave, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ElectronicDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Clave == clave && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<ElectronicInvoice?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ElectronicDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<ElectronicInvoice>?> GetByStatusAsync(string status, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ElectronicDocuments
            .AsNoTracking()
            .Where(x => x.Status == status && x.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ElectronicInvoice>?> GetPendingProcessAsync(string status, Guid tenantId, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ElectronicDocuments
            .AsNoTracking()
            .Where(x => x.Status == status && x.ProcessType == "polling" && x.TenantId == tenantId)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<ElectronicInvoice> Items, int TotalCount)> GetPaginatedAsync(
        Guid tenantId,
        string? status = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? emisorId = null,
        string? receptorId = null,
        bool? requiereCorreccion = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ElectronicDocuments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        if (fechaDesde.HasValue)
            query = query.Where(x => x.FechaEmision >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(x => x.FechaEmision <= fechaHasta.Value);

        if (!string.IsNullOrWhiteSpace(emisorId))
            query = query.Where(x => x.EmisorIdentificacion == emisorId);

        if (!string.IsNullOrWhiteSpace(receptorId))
            query = query.Where(x => x.ReceptorIdentificacion == receptorId);

        if (requiereCorreccion.HasValue)
            query = query.Where(x => x.RequiereCorreccion == requiereCorreccion.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.FechaEmision)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task MarcarParaCorreccionAsync(Guid id, Guid tenantId, string? notas, CancellationToken cancellationToken = default)
    {
        await _dbContext.ElectronicDocuments
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(p => p.RequiereCorreccion, true)
                .SetProperty(p => p.NotasCorreccion, notas)
                .SetProperty(p => p.FechaMarcadoCorreccion, DateTime.UtcNow)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow),
            cancellationToken: cancellationToken);
    }
}
