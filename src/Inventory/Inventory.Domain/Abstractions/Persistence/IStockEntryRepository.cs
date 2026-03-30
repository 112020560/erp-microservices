using Inventory.Domain.Stock;

namespace Inventory.Domain.Abstractions.Persistence;

public interface IStockEntryRepository
{
    Task<StockEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StockEntry?> GetAsync(Guid productId, Guid warehouseId, Guid locationId, Guid? lotId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockEntry>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockEntry>> GetByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockEntry>> GetLowStockAsync(CancellationToken cancellationToken = default);
    void Add(StockEntry stockEntry);
    void Update(StockEntry stockEntry);
}
