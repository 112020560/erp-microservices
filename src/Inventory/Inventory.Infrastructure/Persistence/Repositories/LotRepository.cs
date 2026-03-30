using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Lots;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class LotRepository(InventoryDbContext context) : ILotRepository
{
    public Task<Lot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Lots.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<Lot?> GetByNumberAsync(string lotNumber, Guid productId, CancellationToken cancellationToken = default) =>
        context.Lots.FirstOrDefaultAsync(
            l => l.LotNumber == lotNumber.Trim().ToUpperInvariant() && l.ProductId == productId,
            cancellationToken);

    public Task<bool> ExistsByNumberAsync(string lotNumber, Guid productId, CancellationToken cancellationToken = default) =>
        context.Lots.AnyAsync(
            l => l.LotNumber == lotNumber.Trim().ToUpperInvariant() && l.ProductId == productId,
            cancellationToken);

    public async Task<IReadOnlyList<Lot>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var list = await context.Lots
            .Where(l => l.ProductId == productId)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public void Add(Lot lot) => context.Lots.Add(lot);

    public void Update(Lot lot) => context.Lots.Update(lot);
}
