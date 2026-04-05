using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using FacturaElectronica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Repositorios;

public class TenantHaciendaConfigRepository(AppDbContext context) : ITenantHaciendaConfigRepository
{
    public async Task<TenantHaciendaConfig?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await context.TenantHaciendaConfigs
            .FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);

    public async Task UpsertAsync(TenantHaciendaConfig config, CancellationToken cancellationToken = default)
    {
        var existing = await context.TenantHaciendaConfigs
            .FirstOrDefaultAsync(c => c.TenantId == config.TenantId, cancellationToken);

        if (existing is null)
        {
            config.CreatedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;
            await context.TenantHaciendaConfigs.AddAsync(config, cancellationToken);
        }
        else
        {
            existing.Environment = config.Environment;
            existing.ClientId = config.ClientId;
            existing.UsernameEncrypted = config.UsernameEncrypted;
            existing.PasswordEncrypted = config.PasswordEncrypted;
            existing.AuthUrl = config.AuthUrl;
            existing.SubmitUrl = config.SubmitUrl;
            existing.QueryUrl = config.QueryUrl;
            existing.MaxRetries = config.MaxRetries;
            existing.CallbackUrl = config.CallbackUrl;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }
}
