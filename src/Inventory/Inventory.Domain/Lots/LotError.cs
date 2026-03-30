using SharedKernel;

namespace Inventory.Domain.Lots;

public static class LotError
{
    public static readonly Error LotNumberRequired =
        Error.Failure("Lot.LotNumberRequired", "Lot number is required.");

    public static readonly Error ProductRequired =
        Error.Failure("Lot.ProductRequired", "Product is required.");

    public static readonly Error AlreadyInactive =
        Error.Conflict("Lot.AlreadyInactive", "Lot is already inactive.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Lot.NotFound", $"Lot with id '{id}' was not found.");

    public static Error LotNumberAlreadyExists(string lotNumber, Guid productId) =>
        Error.Conflict("Lot.LotNumberAlreadyExists",
            $"A lot with number '{lotNumber}' already exists for product '{productId}'.");
}
