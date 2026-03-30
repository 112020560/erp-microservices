using Inventory.Domain.Lots;

namespace Inventory.Domain.Abstractions.Persistence;

public interface ILotRepository
{
    Task<Lot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Lot?> GetByNumberAsync(string lotNumber, Guid productId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNumberAsync(string lotNumber, Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lot>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    void Add(Lot lot);
    void Update(Lot lot);
}
