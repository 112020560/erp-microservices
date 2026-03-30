using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.PhysicalInventory;
using SharedKernel;

namespace Inventory.Application.PhysicalInventory.Commands.RecordPhysicalCount;

internal sealed class RecordPhysicalCountCommandHandler(
    IPhysicalCountRepository physicalCountRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RecordPhysicalCountCommand>
{
    public async Task<Result> Handle(RecordPhysicalCountCommand request, CancellationToken cancellationToken)
    {
        var count = await physicalCountRepository.GetByIdAsync(request.CountId, cancellationToken);
        if (count is null)
            return Result.Failure(PhysicalCountError.NotFound(request.CountId));

        var result = count.RecordCount(request.LineId, request.CountedQuantity);
        if (result.IsFailure)
            return result;

        physicalCountRepository.Update(count);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
