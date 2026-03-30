using SharedKernel;

namespace Catalogs.Domain.Brands;

public sealed class ProductBrand
{
    private ProductBrand() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Result<ProductBrand> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductBrand>(BrandError.NameRequired);

        if (name.Length > 100)
            return Result.Failure<ProductBrand>(BrandError.NameTooLong);

        return Result.Success(new ProductBrand
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
    }

    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(BrandError.NameRequired);

        if (name.Length > 100)
            return Result.Failure(BrandError.NameTooLong);

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(BrandError.AlreadyActive);

        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(BrandError.AlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
