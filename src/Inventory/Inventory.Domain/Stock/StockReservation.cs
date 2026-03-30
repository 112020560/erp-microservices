using SharedKernel;

namespace Inventory.Domain.Stock;

public enum ReservationStatus { Active = 0, Released = 1, Cancelled = 2, Expired = 3 }

public sealed class StockReservation
{
    private StockReservation() { }

    public Guid Id { get; private set; }
    public string ReservationNumber { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid LocationId { get; private set; }
    public Guid? LotId { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public static Result<StockReservation> Create(
        string reservationNumber,
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        Guid? lotId,
        decimal quantity,
        Guid salesOrderId,
        DateTimeOffset? expiresAt)
    {
        if (string.IsNullOrWhiteSpace(reservationNumber))
            return Result.Failure<StockReservation>(StockError.ProductRequired);

        if (quantity <= 0)
            return Result.Failure<StockReservation>(StockError.InvalidQuantity);

        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            ReservationNumber = reservationNumber,
            ProductId = productId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            LotId = lotId,
            ReservedQuantity = quantity,
            SalesOrderId = salesOrderId,
            Status = ReservationStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt
        };

        return Result.Success(reservation);
    }

    public Result Release()
    {
        if (Status != ReservationStatus.Active)
            return Result.Failure(StockError.AlreadyReserved);

        Status = ReservationStatus.Released;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != ReservationStatus.Active)
            return Result.Failure(StockError.AlreadyReserved);

        Status = ReservationStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Expire()
    {
        if (Status != ReservationStatus.Active)
            return Result.Failure(StockError.AlreadyReserved);

        Status = ReservationStatus.Expired;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
