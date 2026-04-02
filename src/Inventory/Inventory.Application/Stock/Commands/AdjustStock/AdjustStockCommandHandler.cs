using Inventory.Application.Abstractions.Messaging;
using Inventory.Application.Stock.Commands.ReceiveGoods;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Abstractions.Services;
using Inventory.Domain.Catalog;
using Inventory.Domain.Movements;
using Inventory.Domain.Stock;
using MassTransit;
using SharedKernel;

namespace Inventory.Application.Stock.Commands.AdjustStock;

internal sealed class AdjustStockCommandHandler(
    IInventoryMovementRepository movementRepository,
    IStockEntryRepository stockEntryRepository,
    IProductSnapshotRepository productSnapshotRepository,
    IMovementNumberGenerator movementNumberGenerator,
    IPublishEndpoint eventPublisher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AdjustStockCommand, string>
{
    public async Task<Result<string>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        string movementNumber = await movementNumberGenerator.GenerateAsync("ADJ", cancellationToken);

        var movementResult = InventoryMovement.Create(
            movementNumber,
            MovementType.Adjustment,
            request.WarehouseId,
            null,
            request.Reference,
            request.Notes,
            DateTimeOffset.UtcNow);

        if (movementResult.IsFailure)
            return Result.Failure<string>(movementResult.Error);

        var movement = movementResult.Value;

        var stockEntry = await stockEntryRepository.GetAsync(
            request.ProductId, request.WarehouseId, request.LocationId, request.LotId, cancellationToken);

        if (stockEntry is null)
        {
            var snapshot = await productSnapshotRepository.GetByIdAsync(request.ProductId, cancellationToken);
            decimal minStock = snapshot?.MinimumStock ?? 0;
            decimal reorderPoint = snapshot?.ReorderPoint ?? 0;

            var createResult = StockEntry.Create(
                request.ProductId, request.WarehouseId, request.LocationId, request.LotId, minStock, reorderPoint);

            if (createResult.IsFailure)
                return Result.Failure<string>(createResult.Error);

            stockEntry = createResult.Value;
            stockEntryRepository.Add(stockEntry);
        }

        var adjustResult = stockEntry.AdjustStock(request.NewQuantity, request.UnitCost);
        if (adjustResult.IsFailure)
            return Result.Failure<string>(adjustResult.Error);

        stockEntryRepository.Update(stockEntry);

        var lineResult = movement.AddLine(
            request.ProductId, request.LocationId, null, request.LotId, 
            Math.Abs(request.NewQuantity), request.UnitCost, request.Notes);
        if (lineResult.IsFailure)
            return Result.Failure<string>(lineResult.Error);

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(movementNumber);
    }
}
