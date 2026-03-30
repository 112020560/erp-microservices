using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Stock;
using SharedKernel;
using SharedKernel.Contracts.Inventory;

namespace Inventory.Application.Reservations.Commands.ReleaseStockReservation;

public sealed record StockReservationReleasedMessage : IStockReservationReleased
{
    public required Guid ReservationId { get; init; }
    public required Guid SalesOrderId { get; init; }
    public required DateTimeOffset ReleasedAt { get; init; }
}

internal sealed class ReleaseStockReservationCommandHandler(
    IStockReservationRepository reservationRepository,
    IStockEntryRepository stockEntryRepository,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReleaseStockReservationCommand>
{
    public async Task<Result> Handle(ReleaseStockReservationCommand request, CancellationToken cancellationToken)
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

        var releaseResult = reservation.Release();
        if (releaseResult.IsFailure)
            return releaseResult;

        reservationRepository.Update(reservation);

        await eventPublisher.PublishAsync(new StockReservationReleasedMessage
        {
            ReservationId = reservation.Id,
            SalesOrderId = reservation.SalesOrderId,
            ReleasedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
