using Inventory.Domain.Catalog;
using Inventory.Domain.Stock;
using Inventory.Domain.Warehouses;

namespace Inventory.Domain.Abstractions.Persistence;

public interface IPosStockSearchRepository
{
    Task<(int TotalCount, IReadOnlyList<PosStockRow> Items)> SearchAsync(
        string? q,
        string? sku,
        Guid? warehouseId,
        Guid? categoryId,
        bool onlyAvailable,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public sealed record PosStockRow(
    ProductSnapshot Product,
    IReadOnlyList<(StockEntry Entry, string WarehouseName)> StockEntries);
