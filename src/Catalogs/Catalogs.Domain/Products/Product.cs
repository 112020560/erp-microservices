using SharedKernel;

namespace Catalogs.Domain.Products;

public sealed class Product
{
    private Product() { }

    public Guid Id { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public Guid BrandId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Result<Product> Create(
        string sku,
        string name,
        string? description,
        decimal price,
        string currency,
        Guid categoryId,
        Guid brandId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>(ProductError.NameRequired);

        if (price < 0)
            return Result.Failure<Product>(ProductError.InvalidPrice);

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Product>(ProductError.CurrencyRequired);

        if (categoryId == Guid.Empty)
            return Result.Failure<Product>(ProductError.CategoryRequired);

        if (brandId == Guid.Empty)
            return Result.Failure<Product>(ProductError.BrandRequired);

        var skuResult = Sku.Create(sku);
        if (skuResult.IsFailure)
            return Result.Failure<Product>(skuResult.Error);

        return Result.Success(new Product
        {
            Id = Guid.NewGuid(),
            Sku = skuResult.Value,
            Name = name.Trim(),
            Description = description?.Trim(),
            Price = price,
            Currency = currency.ToUpperInvariant().Trim(),
            CategoryId = categoryId,
            BrandId = brandId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
    }

    public Result Update(string name, string? description, Guid categoryId, Guid brandId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(ProductError.NameRequired);

        if (categoryId == Guid.Empty)
            return Result.Failure(ProductError.CategoryRequired);

        if (brandId == Guid.Empty)
            return Result.Failure(ProductError.BrandRequired);

        Name = name.Trim();
        Description = description?.Trim();
        CategoryId = categoryId;
        BrandId = brandId;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result ChangePrice(decimal newPrice, string currency)
    {
        if (newPrice < 0)
            return Result.Failure(ProductError.InvalidPrice);

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure(ProductError.CurrencyRequired);

        Price = newPrice;
        Currency = currency.ToUpperInvariant().Trim();
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(ProductError.AlreadyActive);

        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(ProductError.AlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
