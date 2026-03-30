using Inventory.Domain.Catalog;

namespace Inventory.Domain.Abstractions.Persistence;

public interface IProductSnapshotRepository
{
    Task<ProductSnapshot?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductSnapshot>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    void Add(ProductSnapshot snapshot);
    void Update(ProductSnapshot snapshot);
}
