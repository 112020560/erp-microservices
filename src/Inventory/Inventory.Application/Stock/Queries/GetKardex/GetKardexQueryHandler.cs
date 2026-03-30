using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Movements;
using SharedKernel;

namespace Inventory.Application.Stock.Queries.GetKardex;

internal sealed class GetKardexQueryHandler(
    IInventoryMovementRepository movementRepository)
    : IQueryHandler<GetKardexQuery, IReadOnlyList<KardexEntryResponse>>
{
    public async Task<Result<IReadOnlyList<KardexEntryResponse>>> Handle(
        GetKardexQuery request,
        CancellationToken cancellationToken)
    {
        var movements = await movementRepository.GetByWarehouseAsync(
            request.WarehouseId ?? Guid.Empty,
            request.From,
            request.To,
            cancellationToken);

        // Filter movements that contain lines for this product
        var relevantMovements = movements
            .Where(m => m.Status == MovementStatus.Confirmed &&
                        m.Lines.Any(l => l.ProductId == request.ProductId))
            .OrderBy(m => m.Date)
            .ToList();

        var entries = new List<KardexEntryResponse>();
        decimal runningBalance = 0;
        decimal runningBalanceCost = 0;

        foreach (var movement in relevantMovements)
        {
            var productLines = movement.Lines.Where(l => l.ProductId == request.ProductId);

            foreach (var line in productLines)
            {
                bool isInbound = movement.MovementType is
                    MovementType.GoodsReceipt or
                    MovementType.Return;

                bool isOutbound = movement.MovementType is
                    MovementType.GoodsIssue or
                    MovementType.Shrinkage;

                bool isTransferIn = movement.MovementType == MovementType.Transfer &&
                                    line.DestinationLocationId.HasValue;

                bool isTransferOut = movement.MovementType == MovementType.Transfer &&
                                     !line.DestinationLocationId.HasValue;

                bool isAdjustment = movement.MovementType is
                    MovementType.Adjustment or
                    MovementType.PhysicalCountAdjust;

                decimal quantityIn = 0;
                decimal quantityOut = 0;

                if (isInbound || isTransferIn)
                {
                    quantityIn = line.Quantity;
                    runningBalance += line.Quantity;
                    runningBalanceCost += line.Quantity * line.UnitCost;
                }
                else if (isOutbound || isTransferOut)
                {
                    quantityOut = line.Quantity;
                    runningBalance -= line.Quantity;
                    runningBalanceCost -= line.Quantity * line.UnitCost;
                }
                else if (isAdjustment)
                {
                    // Determine if positive or negative adjustment based on balance context
                    quantityIn = line.Quantity;
                    runningBalance = line.Quantity; // adjustment sets the balance
                    runningBalanceCost = line.Quantity * line.UnitCost;
                }

                decimal totalCost = line.Quantity * line.UnitCost;

                entries.Add(new KardexEntryResponse(
                    movement.Id,
                    movement.MovementNumber,
                    movement.MovementType.ToString(),
                    movement.Date,
                    quantityIn,
                    quantityOut,
                    runningBalance,
                    line.UnitCost,
                    totalCost,
                    Math.Max(0, runningBalanceCost)));
            }
        }

        return Result.Success<IReadOnlyList<KardexEntryResponse>>(entries.AsReadOnly());
    }
}
