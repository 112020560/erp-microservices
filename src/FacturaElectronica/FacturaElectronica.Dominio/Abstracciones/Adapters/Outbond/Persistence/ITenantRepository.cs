using FacturaElectronica.Dominio.Entidades;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetWithConfigsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<Tenant>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
