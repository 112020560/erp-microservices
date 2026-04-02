using Inventory.Application.Abstractions.Messaging;
using Inventory.Application.Stock.Commands.ReceiveGoods;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Abstractions.Services;
using Inventory.Domain.Catalog;
using Inventory.Domain.Movements;
using Inventory.Domain.Stock;
using MassTransit;
using SharedKernel;

namespace Inventory.Application.Stock.Commands.TransferStock;

internal sealed class TransferStockCommandHandler(
    IInventoryMovementRepository movementRepository,
    IStockEntryRepository stockEntryRepository,
    IProductSnapshotRepository productSnapshotRepository,
    IMovementNumberGenerator movementNumberGenerator,
    IPublishEndpoint eventPublisher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TransferStockCommand, string>
{
    public async Task<Result<string>> Handle(TransferStockCommand request, CancellationToken cancellationToken)
    {
        string movementNumber = await movementNumberGenerator.GenerateAsync("TRF", cancellationToken);

        var movementResult = InventoryMovement.Create(
            movementNumber,
            MovementType.Transfer,
            request.SourceWarehouseId,
            request.DestinationWarehouseId,
            request.Reference,
            request.Notes,
            request.Date);

        if (movementResult.IsFailure)
            return Result.Failure<string>(movementResult.Error);

        var movement = movementResult.Value;

        foreach (var line in request.Lines)
        {
            // Issue from source
            var sourceEntry = await stockEntryRepository.GetAsync(
                line.ProductId, request.SourceWarehouseId, line.SourceLocationId, line.LotId, cancellationToken);

            if (sourceEntry is null)
                return Result.Failure<string>(StockError.StockEntryNotFound(line.ProductId, request.SourceWarehouseId));

            decimal unitCost = sourceEntry.AverageCost;

            var issueResult = sourceEntry.IssueStock(line.Quantity);
            if (issueResult.IsFailure)
                return Result.Failure<string>(issueResult.Error);

            stockEntryRepository.Update(sourceEntry);

            // Receive at destination
            var destEntry = await stockEntryRepository.GetAsync(
                line.ProductId, request.DestinationWarehouseId, line.DestinationLocationId, line.LotId, cancellationToken);

            if (destEntry is null)
            {
                var snapshot = await productSnapshotRepository.GetByIdAsync(line.ProductId, cancellationToken);
                decimal minStock = snapshot?.MinimumStock ?? 0;
                decimal reorderPoint = snapshot?.ReorderPoint ?? 0;

                var createResult = StockEntry.Create(
                    line.ProductId, request.DestinationWarehouseId, line.DestinationLocationId, line.LotId, minStock, reorderPoint);

                if (createResult.IsFailure)
                    return Result.Failure<string>(createResult.Error);

                destEntry = createResult.Value;
                stockEntryRepository.Add(destEntry);
            }

            var receiveResult = destEntry.ReceiveStock(line.Quantity, unitCost);
            if (receiveResult.IsFailure)
                return Result.Failure<string>(receiveResult.Error);

            stockEntryRepository.Update(destEntry);

            var lineResult = movement.AddLine(
                line.ProductId, line.SourceLocationId, line.DestinationLocationId, line.LotId, line.Quantity, unitCost, null);
            if (lineResult.IsFailure)
                return Result.Failure<string>(lineResult.Error);
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(movementNumber);
    }
}
