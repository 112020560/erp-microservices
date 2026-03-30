using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using Inventory.Domain.Abstractions.Services;
using Inventory.Domain.Stock;
using SharedKernel;
using SharedKernel.Contracts.Inventory;

namespace Inventory.Application.Reservations.Commands.CreateStockReservation;

public sealed record StockReservationCreatedMessage : IStockReservationCreated
{
    public required Guid ReservationId { get; init; }
    public required string ReservationNumber { get; init; }
    public required Guid ProductId { get; init; }
    public required Guid WarehouseId { get; init; }
    public required decimal ReservedQuantity { get; init; }
    public required Guid SalesOrderId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

internal sealed class CreateStockReservationCommandHandler(
    IStockEntryRepository stockEntryRepository,
    IStockReservationRepository reservationRepository,
    IMovementNumberGenerator movementNumberGenerator,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateStockReservationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateStockReservationCommand request, CancellationToken cancellationToken)
    {
        var stockEntry = await stockEntryRepository.GetAsync(
            request.ProductId, request.WarehouseId, request.LocationId, request.LotId, cancellationToken);

        if (stockEntry is null)
            return Result.Failure<Guid>(StockError.StockEntryNotFound(request.ProductId, request.WarehouseId));

        if (stockEntry.QuantityAvailable < request.Quantity)
            return Result.Failure<Guid>(StockError.InsufficientAvailableStock);

        string reservationNumber = await movementNumberGenerator.GenerateAsync("RES", cancellationToken);

        var reservationResult = StockReservation.Create(
            reservationNumber,
            request.ProductId,
            request.WarehouseId,
            request.LocationId,
            request.LotId,
            request.Quantity,
            request.SalesOrderId,
            request.ExpiresAt);

        if (reservationResult.IsFailure)
            return Result.Failure<Guid>(reservationResult.Error);

        var reservation = reservationResult.Value;

        var reserveResult = stockEntry.Reserve(request.Quantity);
        if (reserveResult.IsFailure)
            return Result.Failure<Guid>(reserveResult.Error);

        reservationRepository.Add(reservation);
        stockEntryRepository.Update(stockEntry);

        await eventPublisher.PublishAsync(new StockReservationCreatedMessage
        {
            ReservationId = reservation.Id,
            ReservationNumber = reservation.ReservationNumber,
            ProductId = reservation.ProductId,
            WarehouseId = reservation.WarehouseId,
            ReservedQuantity = reservation.ReservedQuantity,
            SalesOrderId = reservation.SalesOrderId,
            CreatedAt = reservation.CreatedAt
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(reservation.Id);
    }
}
