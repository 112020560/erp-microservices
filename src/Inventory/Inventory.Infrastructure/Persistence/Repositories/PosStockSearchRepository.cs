using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Catalog;
using Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class PosStockSearchRepository(InventoryDbContext context) : IPosStockSearchRepository
{
    public async Task<(int TotalCount, IReadOnlyList<PosStockRow> Items)> SearchAsync(
        string? q,
        string? sku,
        Guid? warehouseId,
        Guid? categoryId,
        bool onlyAvailable,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var productsQuery = context.ProductSnapshots
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            productsQuery = productsQuery.Where(p =>
                EF.Functions.ILike(p.Name, $"%{q.Trim()}%") ||
                EF.Functions.ILike(p.Sku, $"%{q.Trim()}%"));

        if (!string.IsNullOrWhiteSpace(sku))
            productsQuery = productsQuery.Where(p =>
                p.Sku == sku.Trim().ToUpperInvariant());

        if (categoryId.HasValue)
            productsQuery = productsQuery.Where(p =>
                p.CategoryId == categoryId.Value);

        if (onlyAvailable)
            productsQuery = productsQuery.Where(p =>
                context.StockEntries.Any(se =>
                    se.ProductId == p.ProductId &&
                    (se.QuantityOnHand - se.QuantityReserved) > 0 &&
                    (!warehouseId.HasValue || se.WarehouseId == warehouseId.Value)));

        var totalCount = await productsQuery.CountAsync(cancellationToken);

        var products = await productsQuery
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
            return (totalCount, []);

        var productIds = products.Select(p => p.ProductId).ToList();

        var stockQuery = context.StockEntries
            .Where(se => productIds.Contains(se.ProductId));

        if (warehouseId.HasValue)
            stockQuery = stockQuery.Where(se => se.WarehouseId == warehouseId.Value);

        var stockEntries = await stockQuery.ToListAsync(cancellationToken);

        var warehouseIds = stockEntries.Select(se => se.WarehouseId).Distinct().ToList();
        var warehouseNames = await context.Warehouses
            .Where(w => warehouseIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.Name, cancellationToken);

        var rows = products.Select(p =>
        {
            var entries = stockEntries
                .Where(se => se.ProductId == p.ProductId)
                .Select(se => (se, warehouseNames.GetValueOrDefault(se.WarehouseId, "—")))
                .ToList()
                .AsReadOnly() as IReadOnlyList<(StockEntry, string)>;

            return new PosStockRow(p, entries);
        }).ToList().AsReadOnly() as IReadOnlyList<PosStockRow>;

        return (totalCount, rows);
    }
}
