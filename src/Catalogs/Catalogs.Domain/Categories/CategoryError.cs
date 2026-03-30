using SharedKernel;

namespace Catalogs.Domain.Categories;

public static class CategoryError
{
    public static readonly Error NameRequired =
        Error.Failure("Category.NameRequired", "Category name is required.");

    public static readonly Error NameTooLong =
        Error.Failure("Category.NameTooLong", "Category name cannot exceed 100 characters.");

    public static readonly Error AlreadyActive =
        Error.Conflict("Category.AlreadyActive", "Category is already active.");

    public static readonly Error AlreadyInactive =
        Error.Conflict("Category.AlreadyInactive", "Category is already inactive.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Category.NotFound", $"Category with id '{id}' was not found.");

    public static Error NameAlreadyExists(string name) =>
        Error.Conflict("Category.NameConflict", $"A category with name '{name}' already exists.");
}
