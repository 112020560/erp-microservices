using SharedKernel;

namespace Retail.Domain.Pricing;

public sealed class CustomerGroup
{
    private readonly List<CustomerGroupMember> _members = [];

    private CustomerGroup() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<CustomerGroupMember> Members => _members.AsReadOnly();

    public static Result<CustomerGroup> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CustomerGroup>(PriceListErrors.CustomerGroupNameRequired);

        return Result.Success(new CustomerGroup
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
            return Result.Failure(PriceListErrors.CustomerGroupNameRequired);

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive) return Result.Failure(PriceListErrors.CustomerGroupAlreadyActive);
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive) return Result.Failure(PriceListErrors.CustomerGroupAlreadyInactive);
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result<CustomerGroupMember> AddMember(Guid customerId)
    {
        if (_members.Any(m => m.CustomerId == customerId))
            return Result.Failure<CustomerGroupMember>(PriceListErrors.CustomerGroupMemberAlreadyExists);

        var member = CustomerGroupMember.Create(Id, customerId);
        _members.Add(member);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(member);
    }

    public Result RemoveMember(Guid customerId)
    {
        var member = _members.FirstOrDefault(m => m.CustomerId == customerId);
        if (member is null) return Result.Failure(PriceListErrors.CustomerGroupMemberNotFound);
        _members.Remove(member);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
