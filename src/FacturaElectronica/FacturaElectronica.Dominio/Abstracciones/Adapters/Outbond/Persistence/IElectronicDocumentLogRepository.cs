using FacturaElectronica.Dominio.Entidades;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;

public interface IElectronicDocumentLogRepository
{
    Task AddLogAsync(ElectronicDocumentLog log, CancellationToken cancellationToken);
    Task<List<ElectronicDocumentLog>> GetByInvoiceIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}
