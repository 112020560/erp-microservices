using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Lots;
using SharedKernel;

namespace Inventory.Application.Lots.Commands.CreateLot;

internal sealed class CreateLotCommandHandler(
    ILotRepository lotRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateLotCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateLotCommand request, CancellationToken cancellationToken)
    {
        if (await lotRepository.ExistsByNumberAsync(request.LotNumber, request.ProductId, cancellationToken))
            return Result.Failure<Guid>(LotError.LotNumberAlreadyExists(request.LotNumber, request.ProductId));

        var result = Lot.Create(
            request.LotNumber,
            request.ProductId,
            request.ManufacturingDate,
            request.ExpirationDate);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var lot = result.Value;
        lotRepository.Add(lot);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(lot.Id);
    }
}
