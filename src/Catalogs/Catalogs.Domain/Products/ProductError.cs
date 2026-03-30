using SharedKernel;

namespace Catalogs.Domain.Products;

public static class ProductError
{
    public static readonly Error NameRequired =
        Error.Failure("Product.NameRequired", "Product name is required.");

    public static readonly Error InvalidPrice =
        Error.Failure("Product.InvalidPrice", "Price cannot be negative.");

    public static readonly Error CurrencyRequired =
        Error.Failure("Product.CurrencyRequired", "Currency is required.");

    public static readonly Error CategoryRequired =
        Error.Failure("Product.CategoryRequired", "Category is required.");

    public static readonly Error BrandRequired =
        Error.Failure("Product.BrandRequired", "Brand is required.");

    public static readonly Error SkuRequired =
        Error.Failure("Product.SkuRequired", "SKU is required.");

    public static readonly Error SkuTooLong =
        Error.Failure("Product.SkuTooLong", "SKU cannot exceed 50 characters.");

    public static readonly Error SkuInvalidFormat =
        Error.Failure("Product.SkuInvalidFormat", "SKU can only contain letters, numbers, hyphens and underscores.");

    public static readonly Error AlreadyActive =
        Error.Conflict("Product.AlreadyActive", "Product is already active.");

    public static readonly Error AlreadyInactive =
        Error.Conflict("Product.AlreadyInactive", "Product is already inactive.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Product.NotFound", $"Product with id '{id}' was not found.");

    public static Error SkuAlreadyExists(string sku) =>
        Error.Conflict("Product.SkuConflict", $"A product with SKU '{sku}' already exists.");
}
