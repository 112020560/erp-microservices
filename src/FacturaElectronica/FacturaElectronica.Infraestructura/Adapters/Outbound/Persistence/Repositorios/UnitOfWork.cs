using FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence.Repositorios;

#nullable disable
public class UnitOfWork : IUnitOfWork
{
    IElectronicInvoiceRepository _electronicInvoiceRepository;
    IElectronicDocumentLogRepository _electronicDocumentLogRepository;
    ITenantRepository _tenantRepository;
    ITenantCertificateRepository _tenantCertificateRepository;
    ITenantHaciendaConfigRepository _tenantHaciendaConfigRepository;
    private IDbContextTransaction _transaction;
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IElectronicInvoiceRepository ElectronicInvoiceRepository
        => _electronicInvoiceRepository ??= new ElectronicInvoiceRepository(_dbContext);

    public IElectronicDocumentLogRepository ElectronicDocumentLogRepository
        => _electronicDocumentLogRepository ??= new ElectronicDocumentLogRepository(_dbContext);

    public ITenantRepository TenantRepository
        => _tenantRepository ??= new TenantRepository(_dbContext);

    public ITenantCertificateRepository TenantCertificateRepository
        => _tenantCertificateRepository ??= new TenantCertificateRepository(_dbContext);

    public ITenantHaciendaConfigRepository TenantHaciendaConfigRepository
        => _tenantHaciendaConfigRepository ??= new TenantHaciendaConfigRepository(_dbContext);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _dbContext?.Dispose();
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
