using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Repositorios;

public class TenantNotificationConfigRepository(AppDbContext dbContext) : ITenantNotificationConfigRepository
{
    public async Task<TenantNotificationConfig?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => await dbContext.TenantNotificationConfigs
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(TenantNotificationConfig config, CancellationToken ct = default)
        => await dbContext.TenantNotificationConfigs.AddAsync(config, ct);

    public void Update(TenantNotificationConfig config)
        => dbContext.TenantNotificationConfigs.Update(config);

    public void Delete(TenantNotificationConfig config)
        => dbContext.TenantNotificationConfigs.Remove(config);
}
