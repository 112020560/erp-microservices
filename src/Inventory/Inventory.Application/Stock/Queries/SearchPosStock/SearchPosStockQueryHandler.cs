using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using SharedKernel;

namespace Inventory.Application.Stock.Queries.SearchPosStock;

internal sealed class SearchPosStockQueryHandler(IPosStockSearchRepository repository)
    : IQueryHandler<SearchPosStockQuery, PosStockPagedResponse>
{
    public async Task<Result<PosStockPagedResponse>> Handle(
        SearchPosStockQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Max(request.Page, 1);

        var (totalCount, rows) = await repository.SearchAsync(
            request.Q,
            request.Sku,
            request.WarehouseId,
            request.CategoryId,
            request.OnlyAvailable,
            page,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = rows.Select(row =>
        {
            var warehouseStock = row.StockEntries
                .GroupBy(t => (t.Entry.WarehouseId, t.WarehouseName))
                .Select(g => new PosWarehouseStockResponse(
                    g.Key.WarehouseId,
                    g.Key.WarehouseName,
                    g.Sum(t => t.Entry.QuantityAvailable),
                    g.Sum(t => t.Entry.QuantityReserved)))
                .OrderBy(w => w.WarehouseName)
                .ToList()
                .AsReadOnly() as IReadOnlyList<PosWarehouseStockResponse>;

            var totalAvailable = warehouseStock.Sum(w => w.Available);
            var totalReserved = warehouseStock.Sum(w => w.Reserved);
            var isLowStock = row.Product.MinimumStock > 0 && totalAvailable <= row.Product.MinimumStock;

            return new PosStockItemResponse(
                row.Product.ProductId,
                row.Product.Sku,
                row.Product.Name,
                row.Product.CategoryId,
                totalAvailable,
                totalReserved,
                isLowStock,
                warehouseStock);
        }).ToList().AsReadOnly() as IReadOnlyList<PosStockItemResponse>;

        return Result.Success(new PosStockPagedResponse(page, pageSize, totalCount, totalPages, items!));
    }
}
