using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using SharedKernel;

namespace Inventory.Application.Stock.Queries.GetStock;

internal sealed class GetStockQueryHandler(
    IStockEntryRepository stockEntryRepository)
    : IQueryHandler<GetStockQuery, IReadOnlyList<StockEntryResponse>>
{
    public async Task<Result<IReadOnlyList<StockEntryResponse>>> Handle(
        GetStockQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Stock.StockEntry> entries;

        if (request.IsLowStock == true)
        {
            entries = await stockEntryRepository.GetLowStockAsync(cancellationToken);
        }
        else if (request.ProductId.HasValue)
        {
            entries = await stockEntryRepository.GetByProductAsync(request.ProductId.Value, cancellationToken);
        }
        else if (request.WarehouseId.HasValue)
        {
            entries = await stockEntryRepository.GetByWarehouseAsync(request.WarehouseId.Value, cancellationToken);
        }
        else
        {
            entries = await stockEntryRepository.GetByWarehouseAsync(Guid.Empty, cancellationToken);
        }

        var response = entries
            .Where(e =>
                (!request.WarehouseId.HasValue || e.WarehouseId == request.WarehouseId.Value) &&
                (!request.ProductId.HasValue || e.ProductId == request.ProductId.Value))
            .Select(e => new StockEntryResponse(
                e.Id,
                e.ProductId,
                e.WarehouseId,
                e.LocationId,
                e.LotId,
                e.QuantityOnHand,
                e.QuantityReserved,
                e.QuantityAvailable,
                e.AverageCost,
                e.MinimumStock,
                e.IsLowStock))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<StockEntryResponse>>(response);
    }
}
