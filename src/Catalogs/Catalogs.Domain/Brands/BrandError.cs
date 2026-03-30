using SharedKernel;

namespace Catalogs.Domain.Brands;

public static class BrandError
{
    public static readonly Error NameRequired =
        Error.Failure("Brand.NameRequired", "Brand name is required.");

    public static readonly Error NameTooLong =
        Error.Failure("Brand.NameTooLong", "Brand name cannot exceed 100 characters.");

    public static readonly Error AlreadyActive =
        Error.Conflict("Brand.AlreadyActive", "Brand is already active.");

    public static readonly Error AlreadyInactive =
        Error.Conflict("Brand.AlreadyInactive", "Brand is already inactive.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Brand.NotFound", $"Brand with id '{id}' was not found.");

    public static Error NameAlreadyExists(string name) =>
        Error.Conflict("Brand.NameConflict", $"A brand with name '{name}' already exists.");
}
