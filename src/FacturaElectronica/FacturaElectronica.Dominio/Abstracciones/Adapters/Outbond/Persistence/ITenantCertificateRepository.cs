using FacturaElectronica.Dominio.Entidades;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;

public interface ITenantCertificateRepository
{
    Task<TenantCertificateConfig?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task UpsertAsync(TenantCertificateConfig config, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
