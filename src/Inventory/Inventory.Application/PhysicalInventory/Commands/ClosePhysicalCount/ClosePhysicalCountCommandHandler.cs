using Inventory.Application.Abstractions.Messaging;
using Inventory.Application.Stock.Commands.ReceiveGoods;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Abstractions.Services;
using Inventory.Domain.Movements;
using Inventory.Domain.PhysicalInventory;
using SharedKernel;
using SharedKernel.Contracts.Inventory;

namespace Inventory.Application.PhysicalInventory.Commands.ClosePhysicalCount;

public sealed record PhysicalInventoryCompletedMessage : IPhysicalInventoryCompleted
{
    public required Guid CountId { get; init; }
    public required string CountNumber { get; init; }
    public required Guid WarehouseId { get; init; }
    public required int TotalLines { get; init; }
    public required int AdjustedLines { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
}

internal sealed class ClosePhysicalCountCommandHandler(
    IPhysicalCountRepository physicalCountRepository,
    IInventoryMovementRepository movementRepository,
    IStockEntryRepository stockEntryRepository,
    IProductSnapshotRepository productSnapshotRepository,
    IMovementNumberGenerator movementNumberGenerator,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ClosePhysicalCountCommand>
{
    public async Task<Result> Handle(ClosePhysicalCountCommand request, CancellationToken cancellationToken)
    {
        var count = await physicalCountRepository.GetByIdAsync(request.CountId, cancellationToken);
        if (count is null)
            return Result.Failure(PhysicalCountError.NotFound(request.CountId));

        var closeResult = count.Close();
        if (closeResult.IsFailure)
            return closeResult;

        int adjustedLines = 0;

        foreach (var line in count.Lines.Where(l => l.Difference.HasValue && l.Difference.Value != 0))
        {
            string movementNumber = await movementNumberGenerator.GenerateAsync("ADJ", cancellationToken);

            var movementResult = InventoryMovement.Create(
                movementNumber,
                MovementType.PhysicalCountAdjust,
                count.WarehouseId,
                null,
                $"PC-{count.CountNumber}",
                $"Physical count adjustment for count {count.CountNumber}",
                DateTimeOffset.UtcNow);

            if (movementResult.IsFailure)
                return movementResult;

            var movement = movementResult.Value;

            var stockEntry = await stockEntryRepository.GetAsync(
                line.ProductId, count.WarehouseId, line.LocationId, line.LotId, cancellationToken);

            decimal newQuantity = line.CountedQuantity!.Value;
            decimal unitCost = stockEntry?.AverageCost ?? 0;

            if (stockEntry is null)
            {
                var snapshot = await productSnapshotRepository.GetByIdAsync(line.ProductId, cancellationToken);
                var createResult = Domain.Stock.StockEntry.Create(
                    line.ProductId, count.WarehouseId, line.LocationId, line.LotId,
                    snapshot?.MinimumStock ?? 0, snapshot?.ReorderPoint ?? 0);

                if (createResult.IsFailure)
                    return createResult;

                stockEntry = createResult.Value;
                stockEntryRepository.Add(stockEntry);
            }

            var adjustResult = stockEntry.AdjustStock(newQuantity, unitCost > 0 ? unitCost : 0);
            if (adjustResult.IsFailure)
                return adjustResult;

            stockEntryRepository.Update(stockEntry);

            var lineQty = Math.Abs(line.Difference!.Value);
            if (lineQty == 0) lineQty = newQuantity;

            movement.AddLine(line.ProductId, line.LocationId, null, line.LotId, lineQty > 0 ? lineQty : 1, unitCost, null);
            movement.Confirm();
            movementRepository.Add(movement);

            line.MarkAsAdjusted();
            adjustedLines++;
        }

        physicalCountRepository.Update(count);

        await eventPublisher.PublishAsync(new PhysicalInventoryCompletedMessage
        {
            CountId = count.Id,
            CountNumber = count.CountNumber,
            WarehouseId = count.WarehouseId,
            TotalLines = count.Lines.Count,
            AdjustedLines = adjustedLines,
            CompletedAt = count.CompletedAt!.Value
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
