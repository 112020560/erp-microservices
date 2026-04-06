using FacturaElectronica.Dominio.Entidades;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;

public interface ITenantNotificationConfigRepository
{
    Task<TenantNotificationConfig?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(TenantNotificationConfig config, CancellationToken ct = default);
    void Update(TenantNotificationConfig config);
    void Delete(TenantNotificationConfig config);
}
