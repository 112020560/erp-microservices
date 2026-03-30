using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Abstractions.Services;
using Inventory.Domain.PhysicalInventory;
using SharedKernel;

namespace Inventory.Application.PhysicalInventory.Commands.StartPhysicalCount;

internal sealed class StartPhysicalCountCommandHandler(
    IPhysicalCountRepository physicalCountRepository,
    IStockEntryRepository stockEntryRepository,
    IMovementNumberGenerator movementNumberGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<StartPhysicalCountCommand, string>
{
    public async Task<Result<string>> Handle(StartPhysicalCountCommand request, CancellationToken cancellationToken)
    {
        string countNumber = await movementNumberGenerator.GenerateAsync("PC", cancellationToken);

        var countResult = PhysicalCount.Create(countNumber, request.WarehouseId, request.Notes);
        if (countResult.IsFailure)
            return Result.Failure<string>(countResult.Error);

        var count = countResult.Value;

        var startResult = count.Start();
        if (startResult.IsFailure)
            return Result.Failure<string>(startResult.Error);

        foreach (var lineDto in request.Lines)
        {
            var stockEntry = await stockEntryRepository.GetAsync(
                lineDto.ProductId, request.WarehouseId, lineDto.LocationId, lineDto.LotId, cancellationToken);

            decimal systemQty = stockEntry?.QuantityOnHand ?? 0;

            var addResult = count.AddLine(lineDto.ProductId, lineDto.LocationId, lineDto.LotId, systemQty);
            if (addResult.IsFailure)
                return Result.Failure<string>(addResult.Error);
        }

        physicalCountRepository.Add(count);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(countNumber);
    }
}
