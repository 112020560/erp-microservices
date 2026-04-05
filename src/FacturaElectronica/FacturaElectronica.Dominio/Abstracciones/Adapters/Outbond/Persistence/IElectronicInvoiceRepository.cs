using FacturaElectronica.Dominio.Entidades;

namespace FacturaElectronica.Dominio.Abstracciones.Adapters.Outbond.Persistence;

public interface IElectronicInvoiceRepository
{
    Task AddAsync(ElectronicInvoice electronicInvoice, CancellationToken cancellationToken = default);
    Task<ElectronicInvoice?> GetByClaveAsync(string clave, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ElectronicInvoice?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task UpdateAsync(ElectronicInvoice electronicInvoice, CancellationToken cancellationToken = default);
    Task<List<ElectronicInvoice>?> GetByStatusAsync(string status, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<ElectronicInvoice>?> GetPendingProcessAsync(string status, Guid tenantId, int limit = 10, CancellationToken cancellationToken = default);
    Task<(List<ElectronicInvoice> Items, int TotalCount)> GetPaginatedAsync(
        Guid tenantId,
        string? status = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? emisorId = null,
        string? receptorId = null,
        bool? requiereCorreccion = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task MarcarParaCorreccionAsync(Guid id, Guid tenantId, string? notas, CancellationToken cancellationToken = default);
}
