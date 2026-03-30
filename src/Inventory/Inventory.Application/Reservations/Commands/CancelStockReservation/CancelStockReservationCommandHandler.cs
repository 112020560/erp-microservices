using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Stock;
using SharedKernel;

namespace Inventory.Application.Reservations.Commands.CancelStockReservation;

internal sealed class CancelStockReservationCommandHandler(
    IStockReservationRepository reservationRepository,
    IStockEntryRepository stockEntryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CancelStockReservationCommand>
{
    public async Task<Result> Handle(CancelStockReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken);
        if (reservation is null)
            return Result.Failure(StockError.ReservationNotFound(request.ReservationId));

        var stockEntry = await stockEntryRepository.GetAsync(
            reservation.ProductId, reservation.WarehouseId, reservation.LocationId, reservation.LotId, cancellationToken);

        if (stockEntry is not null)
        {
            stockEntry.ReleaseReservation(reservation.ReservedQuantity);
            stockEntryRepository.Update(stockEntry);
        }

        var cancelResult = reservation.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult;

        reservationRepository.Update(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
