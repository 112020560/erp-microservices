namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;

public interface IUnitOfWork : IDisposable
{
    public IElectronicInvoiceRepository ElectronicInvoiceRepository { get; }
    public IElectronicDocumentLogRepository ElectronicDocumentLogRepository { get; }
    public ITenantRepository TenantRepository { get; }
    public ITenantCertificateRepository TenantCertificateRepository { get; }
    public ITenantHaciendaConfigRepository TenantHaciendaConfigRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
