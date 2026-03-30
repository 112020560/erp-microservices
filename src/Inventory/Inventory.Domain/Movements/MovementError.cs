using SharedKernel;

namespace Inventory.Domain.Movements;

public static class MovementError
{
    public static readonly Error MovementNumberRequired =
        Error.Failure("Movement.MovementNumberRequired", "Movement number is required.");

    public static readonly Error WarehouseRequired =
        Error.Failure("Movement.WarehouseRequired", "Warehouse is required.");

    public static readonly Error InvalidMovementType =
        Error.Failure("Movement.InvalidMovementType", "Invalid movement type.");

    public static readonly Error AlreadyConfirmed =
        Error.Conflict("Movement.AlreadyConfirmed", "Movement is already confirmed.");

    public static readonly Error AlreadyCancelled =
        Error.Conflict("Movement.AlreadyCancelled", "Movement is already cancelled.");

    public static readonly Error NoLines =
        Error.Failure("Movement.NoLines", "Movement must have at least one line.");

    public static readonly Error CannotModifyConfirmed =
        Error.Conflict("Movement.CannotModifyConfirmed", "Cannot modify a confirmed or cancelled movement.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Movement.NotFound", $"Movement with id '{id}' was not found.");

    public static Error NotFoundByNumber(string number) =>
        Error.NotFound("Movement.NotFoundByNumber", $"Movement with number '{number}' was not found.");
}
