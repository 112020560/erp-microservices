using SharedKernel;

namespace Inventory.Domain.Catalog;

public sealed class ProductSnapshot
{
    private ProductSnapshot() { }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public Guid BrandId { get; private set; }
    public TrackingType TrackingType { get; private set; }
    public decimal MinimumStock { get; private set; }
    public decimal ReorderPoint { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset LastSyncedAt { get; private set; }

    public static ProductSnapshot CreateFromCatalog(
        Guid productId,
        string sku,
        string name,
        Guid categoryId,
        Guid brandId)
    {
        return new ProductSnapshot
        {
            ProductId = productId,
            Sku = sku,
            Name = name,
            CategoryId = categoryId,
            BrandId = brandId,
            TrackingType = TrackingType.None,
            MinimumStock = 0,
            ReorderPoint = 0,
            IsActive = true,
            LastSyncedAt = DateTimeOffset.UtcNow
        };
    }

    public void SyncFromCatalog(string name, Guid categoryId, Guid brandId, bool isActive, DateTimeOffset syncedAt)
    {
        Name = name;
        CategoryId = categoryId;
        BrandId = brandId;
        IsActive = isActive;
        LastSyncedAt = syncedAt;
    }

    public Result ConfigureInventory(TrackingType trackingType, decimal minimumStock, decimal reorderPoint)
    {
        if (minimumStock < 0)
            return Result.Failure(Error.Failure("ProductSnapshot.InvalidMinimumStock", "Minimum stock cannot be negative."));

        if (reorderPoint < 0)
            return Result.Failure(Error.Failure("ProductSnapshot.InvalidReorderPoint", "Reorder point cannot be negative."));

        TrackingType = trackingType;
        MinimumStock = minimumStock;
        ReorderPoint = reorderPoint;
        return Result.Success();
    }
}
