using Inventory.Domain.Warehouses;

namespace Inventory.Domain.Abstractions.Persistence;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warehouse>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    void Add(Warehouse warehouse);
    void Update(Warehouse warehouse);
}
