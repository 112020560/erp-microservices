using SharedKernel;

namespace Retail.Domain.Pricing;

public sealed class Promotion
{
    private readonly List<PromotionCondition> _conditions = [];
    private readonly List<PromotionAction> _actions = [];
    private readonly List<PromotionUsage> _usages = [];

    private Promotion() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CouponCode { get; private set; }     // null = automatic
    public bool IsActive { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public int? MaxUses { get; private set; }
    public int? MaxUsesPerCustomer { get; private set; }
    public int UsedCount { get; private set; }
    public int Priority { get; private set; }
    public bool CanStackWithOthers { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<PromotionCondition> Conditions => _conditions.AsReadOnly();
    public IReadOnlyList<PromotionAction> Actions => _actions.AsReadOnly();
    public IReadOnlyList<PromotionUsage> Usages => _usages.AsReadOnly();

    public bool IsAutomatic => CouponCode is null;

    public static Result<Promotion> Create(
        string name,
        string? description,
        string? couponCode,
        DateTimeOffset? validFrom,
        DateTimeOffset? validTo,
        int? maxUses,
        int? maxUsesPerCustomer,
        int priority,
        bool canStackWithOthers)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Promotion>(PromotionErrors.NameRequired);

        if (validFrom.HasValue && validTo.HasValue && validTo <= validFrom)
            return Result.Failure<Promotion>(PromotionErrors.InvalidDateRange);

        return Result.Success(new Promotion
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            CouponCode = string.IsNullOrWhiteSpace(couponCode) ? null : couponCode.Trim().ToUpperInvariant(),
            IsActive = true,
            ValidFrom = validFrom,
            ValidTo = validTo,
            MaxUses = maxUses,
            MaxUsesPerCustomer = maxUsesPerCustomer,
            UsedCount = 0,
            Priority = priority,
            CanStackWithOthers = canStackWithOthers,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
    }

    public Result Update(string name, string? description, DateTimeOffset? validFrom, DateTimeOffset? validTo,
        int? maxUses, int? maxUsesPerCustomer, int priority, bool canStackWithOthers)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(PromotionErrors.NameRequired);

        if (validFrom.HasValue && validTo.HasValue && validTo <= validFrom)
            return Result.Failure(PromotionErrors.InvalidDateRange);

        Name = name.Trim();
        Description = description?.Trim();
        ValidFrom = validFrom;
        ValidTo = validTo;
        MaxUses = maxUses;
        MaxUsesPerCustomer = maxUsesPerCustomer;
        Priority = priority;
        CanStackWithOthers = canStackWithOthers;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive) return Result.Failure(PromotionErrors.AlreadyActive);
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive) return Result.Failure(PromotionErrors.AlreadyInactive);
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result<PromotionCondition> AddCondition(PromotionConditionType type, decimal? decimalValue, Guid? referenceId, int? intValue)
    {
        // Validate: reference-based types need ReferenceId
        if (type is PromotionConditionType.ContainsProduct or PromotionConditionType.ContainsCategory or PromotionConditionType.CustomerInGroup)
        {
            if (!referenceId.HasValue)
                return Result.Failure<PromotionCondition>(PromotionErrors.ConditionReferenceIdRequired);
        }
        // Validate: threshold types need DecimalValue
        if (type is PromotionConditionType.MinCartTotal or PromotionConditionType.MinCartQuantity)
        {
            if (!decimalValue.HasValue || decimalValue <= 0)
                return Result.Failure<PromotionCondition>(PromotionErrors.ConditionValueRequired);
        }
        if (type == PromotionConditionType.MinItemCount)
        {
            if (!intValue.HasValue || intValue <= 0)
                return Result.Failure<PromotionCondition>(PromotionErrors.ConditionValueRequired);
        }

        var condition = PromotionCondition.Create(Id, type, decimalValue, referenceId, intValue);
        _conditions.Add(condition);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(condition);
    }

    public Result RemoveCondition(Guid conditionId)
    {
        var condition = _conditions.FirstOrDefault(c => c.Id == conditionId);
        if (condition is null) return Result.Failure(PromotionErrors.ConditionNotFound);
        _conditions.Remove(condition);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result<PromotionAction> AddAction(PromotionActionType type, decimal? discountPercentage, decimal? discountAmount,
        Guid? targetReferenceId, int? buyQuantity, int? getQuantity, Guid? buyReferenceId, Guid? getReferenceId)
    {
        // Validate by action type
        if (type is PromotionActionType.CartPercentageDiscount or PromotionActionType.ProductPercentageDiscount)
        {
            if (!discountPercentage.HasValue || discountPercentage <= 0 || discountPercentage > 100)
                return Result.Failure<PromotionAction>(PromotionErrors.ActionInvalidPercentage);
        }
        if (type is PromotionActionType.CartFixedDiscount or PromotionActionType.ProductFixedDiscount)
        {
            if (!discountAmount.HasValue || discountAmount <= 0)
                return Result.Failure<PromotionAction>(PromotionErrors.ActionInvalidAmount);
        }
        if (type is PromotionActionType.ProductPercentageDiscount or PromotionActionType.ProductFixedDiscount)
        {
            if (!targetReferenceId.HasValue)
                return Result.Failure<PromotionAction>(PromotionErrors.ActionTargetRequired);
        }
        if (type == PromotionActionType.BuyXGetYFree)
        {
            if (!buyQuantity.HasValue || buyQuantity <= 0 || !getQuantity.HasValue || getQuantity <= 0)
                return Result.Failure<PromotionAction>(PromotionErrors.ActionInvalidBogoQuantities);
            if (!buyReferenceId.HasValue || !getReferenceId.HasValue)
                return Result.Failure<PromotionAction>(PromotionErrors.ActionTargetRequired);
        }

        var action = PromotionAction.Create(Id, type, discountPercentage, discountAmount,
            targetReferenceId, buyQuantity, getQuantity, buyReferenceId, getReferenceId);
        _actions.Add(action);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(action);
    }

    public Result RemoveAction(Guid actionId)
    {
        var action = _actions.FirstOrDefault(a => a.Id == actionId);
        if (action is null) return Result.Failure(PromotionErrors.ActionNotFound);
        _actions.Remove(action);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result RecordUsage(Guid? customerId, string? externalOrderId)
    {
        if (MaxUses.HasValue && UsedCount >= MaxUses.Value)
            return Result.Failure(PromotionErrors.MaxUsesReached);

        if (MaxUsesPerCustomer.HasValue && customerId.HasValue)
        {
            var customerUsages = _usages.Count(u => u.CustomerId == customerId.Value);
            if (customerUsages >= MaxUsesPerCustomer.Value)
                return Result.Failure(PromotionErrors.MaxUsesPerCustomerReached);
        }

        _usages.Add(PromotionUsage.Create(Id, customerId, externalOrderId));
        UsedCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public bool IsValidAt(DateTimeOffset date)
    {
        if (!IsActive) return false;
        if (ValidFrom.HasValue && date < ValidFrom.Value) return false;
        if (ValidTo.HasValue && date > ValidTo.Value) return false;
        if (MaxUses.HasValue && UsedCount >= MaxUses.Value) return false;
        return true;
    }
}
