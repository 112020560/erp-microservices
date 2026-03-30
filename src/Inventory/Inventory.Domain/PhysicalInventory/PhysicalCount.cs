using SharedKernel;

namespace Inventory.Domain.PhysicalInventory;

public enum PhysicalCountStatus { Draft = 0, InProgress = 1, PendingApproval = 2, Closed = 3 }

public sealed class PhysicalCount
{
    private readonly List<CountLine> _lines = [];

    private PhysicalCount() { }

    public Guid Id { get; private set; }
    public string CountNumber { get; private set; } = string.Empty;
    public Guid WarehouseId { get; private set; }
    public PhysicalCountStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyList<CountLine> Lines => _lines.AsReadOnly();

    public static Result<PhysicalCount> Create(string countNumber, Guid warehouseId, string? notes)
    {
        if (string.IsNullOrWhiteSpace(countNumber))
            return Result.Failure<PhysicalCount>(PhysicalCountError.CountNumberRequired);

        if (warehouseId == Guid.Empty)
            return Result.Failure<PhysicalCount>(PhysicalCountError.WarehouseRequired);

        var count = new PhysicalCount
        {
            Id = Guid.NewGuid(),
            CountNumber = countNumber,
            WarehouseId = warehouseId,
            Status = PhysicalCountStatus.Draft,
            StartedAt = DateTimeOffset.UtcNow,
            Notes = notes?.Trim()
        };

        return Result.Success(count);
    }

    public Result Start()
    {
        if (Status != PhysicalCountStatus.Draft)
            return Result.Failure(PhysicalCountError.AlreadyStarted);

        Status = PhysicalCountStatus.InProgress;
        return Result.Success();
    }

    public Result AddLine(Guid productId, Guid locationId, Guid? lotId, decimal systemQuantity)
    {
        if (Status != PhysicalCountStatus.InProgress)
            return Result.Failure(PhysicalCountError.CannotAddLines);

        var result = CountLine.Create(Id, productId, locationId, lotId, systemQuantity);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        _lines.Add(result.Value);
        return Result.Success();
    }

    public Result RecordCount(Guid lineId, decimal countedQuantity)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
            return Result.Failure(PhysicalCountError.LineNotFound(lineId));

        return line.RecordCount(countedQuantity);
    }

    public Result SubmitForApproval()
    {
        if (Status != PhysicalCountStatus.InProgress)
            return Result.Failure(
                Error.Conflict("PhysicalCount.InvalidStatus", "Physical count must be InProgress to submit for approval."));

        Status = PhysicalCountStatus.PendingApproval;
        return Result.Success();
    }

    public Result Close()
    {
        if (Status != PhysicalCountStatus.PendingApproval)
            return Result.Failure(
                Error.Conflict("PhysicalCount.InvalidStatus", "Physical count must be PendingApproval to close."));

        Status = PhysicalCountStatus.Closed;
        CompletedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
