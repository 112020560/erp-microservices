using Inventory.Domain.PhysicalInventory;

namespace Inventory.Domain.Abstractions.Persistence;

public interface IPhysicalCountRepository
{
    Task<PhysicalCount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhysicalCount>> GetByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<int> GetNextSequenceAsync(CancellationToken cancellationToken = default);
    void Add(PhysicalCount physicalCount);
    void Update(PhysicalCount physicalCount);
}
