using SharedKernel;

namespace Inventory.Domain.Warehouses;

public sealed class Warehouse
{
    private readonly List<Location> _locations = [];

    private Warehouse() { }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<Location> Locations => _locations.AsReadOnly();

    public static Result<Warehouse> Create(string code, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Warehouse>(WarehouseError.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Warehouse>(WarehouseError.NameRequired);

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        return Result.Success(warehouse);
    }

    public Result AddLocation(string aisle, string rack, string level, string? name)
    {
        var result = Location.Create(Id, aisle, rack, level, name);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        _locations.Add(result.Value);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result<Location> AddLocationAndReturn(string aisle, string rack, string level, string? name)
    {
        var result = Location.Create(Id, aisle, rack, level, name);
        if (result.IsFailure)
            return Result.Failure<Location>(result.Error);

        _locations.Add(result.Value);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(result.Value);
    }

    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(WarehouseError.NameRequired);

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(WarehouseError.AlreadyActive);

        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(WarehouseError.AlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result<Location> GetLocation(Guid locationId)
    {
        var location = _locations.FirstOrDefault(l => l.Id == locationId);
        return location is not null
            ? Result.Success(location)
            : Result.Failure<Location>(WarehouseError.LocationNotFound(locationId));
    }
}
