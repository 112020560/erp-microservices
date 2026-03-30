using SharedKernel;

namespace Inventory.Domain.PhysicalInventory;

public static class PhysicalCountError
{
    public static readonly Error CountNumberRequired =
        Error.Failure("PhysicalCount.CountNumberRequired", "Count number is required.");

    public static readonly Error WarehouseRequired =
        Error.Failure("PhysicalCount.WarehouseRequired", "Warehouse is required.");

    public static readonly Error AlreadyStarted =
        Error.Conflict("PhysicalCount.AlreadyStarted", "Physical count has already started.");

    public static readonly Error AlreadyClosed =
        Error.Conflict("PhysicalCount.AlreadyClosed", "Physical count is already closed.");

    public static readonly Error CannotAddLines =
        Error.Conflict("PhysicalCount.CannotAddLines", "Cannot add lines unless the count is InProgress.");

    public static readonly Error LineAlreadyCounted =
        Error.Conflict("PhysicalCount.LineAlreadyCounted", "This line has already been counted.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("PhysicalCount.NotFound", $"Physical count with id '{id}' was not found.");

    public static Error LineNotFound(Guid id) =>
        Error.NotFound("PhysicalCount.LineNotFound", $"Count line with id '{id}' was not found.");
}
