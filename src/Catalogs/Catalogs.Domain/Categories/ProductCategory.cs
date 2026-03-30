using SharedKernel;

namespace Catalogs.Domain.Categories;

public sealed class ProductCategory
{
    private ProductCategory() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Result<ProductCategory> Create(string name, string? description, Guid? parentCategoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductCategory>(CategoryError.NameRequired);

        if (name.Length > 100)
            return Result.Failure<ProductCategory>(CategoryError.NameTooLong);

        return Result.Success(new ProductCategory
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ParentCategoryId = parentCategoryId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
    }

    public Result Update(string name, string? description, Guid? parentCategoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(CategoryError.NameRequired);

        if (name.Length > 100)
            return Result.Failure(CategoryError.NameTooLong);

        Name = name.Trim();
        Description = description?.Trim();
        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(CategoryError.AlreadyActive);

        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(CategoryError.AlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
