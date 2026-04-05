using FacturaElectronica.Dominio.Entidades;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;

public interface ITenantHaciendaConfigRepository
{
    Task<TenantHaciendaConfig?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task UpsertAsync(TenantHaciendaConfig config, CancellationToken cancellationToken = default);
}
