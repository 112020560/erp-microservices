using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Repositorios;

public class TenantCertificateRepository(AppDbContext context) : ITenantCertificateRepository
{
    public async Task<TenantCertificateConfig?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await context.TenantCertificateConfigs
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.IsActive, cancellationToken);

    public async Task UpsertAsync(TenantCertificateConfig config, CancellationToken cancellationToken = default)
    {
        var existing = await context.TenantCertificateConfigs
            .FirstOrDefaultAsync(c => c.TenantId == config.TenantId, cancellationToken);

        if (existing is null)
        {
            config.CreatedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;
            await context.TenantCertificateConfigs.AddAsync(config, cancellationToken);
        }
        else
        {
            existing.CertificatePath = config.CertificatePath;
            existing.CertificateKeyEncrypted = config.CertificateKeyEncrypted;
            existing.ValidFrom = config.ValidFrom;
            existing.ValidUntil = config.ValidUntil;
            existing.IsActive = config.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> DeleteAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var config = await context.TenantCertificateConfigs
            .FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);
        if (config is null) return false;
        context.TenantCertificateConfigs.Remove(config);
        return true;
    }
}
