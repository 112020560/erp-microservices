using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Repositorios;

public class TenantRepository(AppDbContext context) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await context.Tenants.FindAsync([tenantId], cancellationToken);

    public async Task<Tenant?> GetWithConfigsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await context.Tenants
            .Include(t => t.EmitterConfig)
            .Include(t => t.CertificateConfig)
            .Include(t => t.HaciendaConfig)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

    public async Task<bool> ExistsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await context.Tenants.AnyAsync(t => t.Id == tenantId && t.IsActive, cancellationToken);

    public async Task<List<Tenant>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => await context.Tenants
            .AsNoTracking()
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        => await context.Tenants.AddAsync(tenant, cancellationToken);

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        tenant.UpdatedAt = DateTime.UtcNow;
        context.Tenants.Update(tenant);
        return Task.CompletedTask;
    }
}
