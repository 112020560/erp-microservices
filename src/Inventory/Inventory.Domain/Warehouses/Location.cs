using SharedKernel;

namespace Inventory.Domain.Warehouses;

public sealed class Location
{
    private Location() { }

    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string Aisle { get; private set; } = string.Empty;
    public string Rack { get; private set; } = string.Empty;
    public string Level { get; private set; } = string.Empty;
    public string? Name { get; private set; }
    public bool IsActive { get; private set; }

    public string Code => $"{Aisle}-{Rack}-{Level}";

    internal static Result<Location> Create(Guid warehouseId, string aisle, string rack, string level, string? name)
    {
        if (string.IsNullOrWhiteSpace(aisle))
            return Result.Failure<Location>(WarehouseError.AisleRequired);

        if (string.IsNullOrWhiteSpace(rack))
            return Result.Failure<Location>(WarehouseError.RackRequired);

        if (string.IsNullOrWhiteSpace(level))
            return Result.Failure<Location>(WarehouseError.LevelRequired);

        var location = new Location
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouseId,
            Aisle = aisle.Trim().ToUpperInvariant(),
            Rack = rack.Trim().ToUpperInvariant(),
            Level = level.Trim().ToUpperInvariant(),
            Name = name?.Trim(),
            IsActive = true
        };

        return Result.Success(location);
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(WarehouseError.AlreadyActive);

        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(WarehouseError.AlreadyInactive);

        IsActive = false;
        return Result.Success();
    }
}
