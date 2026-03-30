using SharedKernel;

namespace Inventory.Domain.Stock;

public static class StockError
{
    public static readonly Error InsufficientStock =
        Error.Failure("Stock.InsufficientStock", "Insufficient stock on hand.");

    public static readonly Error InsufficientAvailableStock =
        Error.Failure("Stock.InsufficientAvailableStock", "Insufficient available stock (considering reservations).");

    public static readonly Error InvalidQuantity =
        Error.Failure("Stock.InvalidQuantity", "Quantity must be greater than zero.");

    public static readonly Error InvalidCost =
        Error.Failure("Stock.InvalidCost", "Unit cost cannot be negative.");

    public static readonly Error ProductRequired =
        Error.Failure("Stock.ProductRequired", "Product is required.");

    public static readonly Error WarehouseRequired =
        Error.Failure("Stock.WarehouseRequired", "Warehouse is required.");

    public static readonly Error LocationRequired =
        Error.Failure("Stock.LocationRequired", "Location is required.");

    public static readonly Error AlreadyReserved =
        Error.Conflict("Stock.AlreadyReserved", "Reservation is not in Active status.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Stock.NotFound", $"Stock entry with id '{id}' was not found.");

    public static Error ReservationNotFound(Guid id) =>
        Error.NotFound("Stock.ReservationNotFound", $"Stock reservation with id '{id}' was not found.");

    public static Error StockEntryNotFound(Guid productId, Guid warehouseId) =>
        Error.NotFound("Stock.StockEntryNotFound",
            $"Stock entry for product '{productId}' in warehouse '{warehouseId}' was not found.");
}
