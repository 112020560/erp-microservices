using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Abstractions.Services;
using Inventory.Domain.Catalog;
using Inventory.Domain.Movements;
using Inventory.Domain.Stock;
using MassTransit;
using SharedKernel;

namespace Inventory.Application.Stock.Commands.ReceiveGoods;

internal sealed class ReceiveGoodsCommandHandler(
    IInventoryMovementRepository movementRepository,
    IStockEntryRepository stockEntryRepository,
    IProductSnapshotRepository productSnapshotRepository,
    IMovementNumberGenerator movementNumberGenerator,
    IPublishEndpoint eventPublisher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReceiveGoodsCommand, string>
{
    public async Task<Result<string>> Handle(ReceiveGoodsCommand request, CancellationToken cancellationToken)
    {
        string movementNumber = await movementNumberGenerator.GenerateAsync("GR", cancellationToken);

        var movementResult = InventoryMovement.Create(
            movementNumber,
            MovementType.GoodsReceipt,
            request.WarehouseId,
            null,
            request.Reference,
            request.Notes,
            request.Date);

        if (movementResult.IsFailure)
            return Result.Failure<string>(movementResult.Error);

        var movement = movementResult.Value;
        var lowStockItems = new List<(Guid ProductId, decimal OnHand, decimal MinStock)>();

        foreach (var line in request.Lines)
        {
            var stockEntry = await stockEntryRepository.GetAsync(
                line.ProductId, request.WarehouseId, line.LocationId, line.LotId, cancellationToken);

            if (stockEntry is null)
            {
                var snapshot = await productSnapshotRepository.GetByIdAsync(line.ProductId, cancellationToken);
                decimal minStock = snapshot?.MinimumStock ?? 0;
                decimal reorderPoint = snapshot?.ReorderPoint ?? 0;

                var createResult = StockEntry.Create(
                    line.ProductId, request.WarehouseId, line.LocationId, line.LotId, minStock, reorderPoint);

                if (createResult.IsFailure)
                    return Result.Failure<string>(createResult.Error);

                stockEntry = createResult.Value;
                stockEntryRepository.Add(stockEntry);
            }

            var receiveResult = stockEntry.ReceiveStock(line.Quantity, line.UnitCost);
            if (receiveResult.IsFailure)
                return Result.Failure<string>(receiveResult.Error);

            stockEntryRepository.Update(stockEntry);

            var lineResult = movement.AddLine(
                line.ProductId, line.LocationId, null, line.LotId, line.Quantity, line.UnitCost, null);
            if (lineResult.IsFailure)
                return Result.Failure<string>(lineResult.Error);

            if (stockEntry.IsLowStock)
                lowStockItems.Add((stockEntry.ProductId, stockEntry.QuantityOnHand, stockEntry.MinimumStock));
        }

        var confirmResult = movement.Confirm();
        if (confirmResult.IsFailure)
            return Result.Failure<string>(confirmResult.Error);

        movementRepository.Add(movement);

        await eventPublisher.Publish(new StockMovementConfirmedMessage
        {
            MovementId = movement.Id,
            MovementNumber = movement.MovementNumber,
            MovementType = (int)movement.MovementType,
            WarehouseId = movement.WarehouseId,
            ConfirmedAt = movement.ConfirmedAt!.Value
        }, cancellationToken);

        foreach (var (productId, onHand, minStock) in lowStockItems)
        {
            var snapshot = await productSnapshotRepository.GetByIdAsync(productId, cancellationToken);
            await eventPublisher.Publish(new LowStockDetectedMessage
            {
                ProductId = productId,
                Sku = snapshot?.Sku ?? string.Empty,
                ProductName = snapshot?.Name ?? string.Empty,
                WarehouseId = request.WarehouseId,
                QuantityOnHand = onHand,
                MinimumStock = minStock,
                DetectedAt = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(movementNumber);
    }
}
