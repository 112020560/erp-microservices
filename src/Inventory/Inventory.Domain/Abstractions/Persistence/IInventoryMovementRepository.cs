using Inventory.Domain.Movements;

namespace Inventory.Domain.Abstractions.Persistence;

public interface IInventoryMovementRepository
{
    Task<InventoryMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryMovement>> GetByWarehouseAsync(Guid warehouseId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default);
    Task<int> GetNextSequenceAsync(CancellationToken cancellationToken = default);
    void Add(InventoryMovement movement);
    void Update(InventoryMovement movement);
}
