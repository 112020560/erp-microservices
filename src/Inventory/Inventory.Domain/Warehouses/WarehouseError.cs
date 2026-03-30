using SharedKernel;

namespace Inventory.Domain.Warehouses;

public static class WarehouseError
{
    public static readonly Error CodeRequired =
        Error.Failure("Warehouse.CodeRequired", "Warehouse code is required.");

    public static readonly Error NameRequired =
        Error.Failure("Warehouse.NameRequired", "Warehouse name is required.");

    public static readonly Error AisleRequired =
        Error.Failure("Warehouse.AisleRequired", "Aisle is required.");

    public static readonly Error RackRequired =
        Error.Failure("Warehouse.RackRequired", "Rack is required.");

    public static readonly Error LevelRequired =
        Error.Failure("Warehouse.LevelRequired", "Level is required.");

    public static readonly Error AlreadyActive =
        Error.Conflict("Warehouse.AlreadyActive", "Warehouse is already active.");

    public static readonly Error AlreadyInactive =
        Error.Conflict("Warehouse.AlreadyInactive", "Warehouse is already inactive.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Warehouse.NotFound", $"Warehouse with id '{id}' was not found.");

    public static Error LocationNotFound(Guid id) =>
        Error.NotFound("Warehouse.LocationNotFound", $"Location with id '{id}' was not found.");

    public static Error CodeAlreadyExists(string code) =>
        Error.Conflict("Warehouse.CodeAlreadyExists", $"A warehouse with code '{code}' already exists.");
}
