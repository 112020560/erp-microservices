using SharedKernel;

namespace Retail.Domain.Pricing;

public static class PriceListErrors
{
    public static readonly Error NameRequired = Error.Failure("PriceList.NameRequired", "Price list name is required.");
    public static readonly Error InvalidCurrency = Error.Failure("PriceList.InvalidCurrency", "Currency must be a valid 3-letter ISO code.");
    public static readonly Error InvalidDateRange = Error.Failure("PriceList.InvalidDateRange", "ValidTo must be after ValidFrom.");
    public static readonly Error AlreadyActive = Error.Failure("PriceList.AlreadyActive", "Price list is already active.");
    public static readonly Error AlreadyInactive = Error.Failure("PriceList.AlreadyInactive", "Price list is already inactive.");
    public static readonly Error InvalidPrice = Error.Failure("PriceList.InvalidPrice", "Price cannot be negative.");
    public static readonly Error InvalidDiscount = Error.Failure("PriceList.InvalidDiscount", "Discount percentage must be between 0 and 100.");
    public static readonly Error InvalidQuantity = Error.Failure("PriceList.InvalidQuantity", "Minimum quantity cannot be negative.");
    public static readonly Error InvalidQuantityRange = Error.Failure("PriceList.InvalidQuantityRange", "MaxQuantity must be greater than MinQuantity.");
    public static readonly Error ReferenceIdRequired = Error.Failure("PriceList.ReferenceIdRequired", "ReferenceId is required for Product or Category items.");
    public static readonly Error NotFound = Error.NotFound("PriceList.NotFound", "Price list not found.");

    public static Error ItemNotFound(Guid itemId) =>
        Error.NotFound("PriceList.ItemNotFound", $"Price list item '{itemId}' not found.");

    public static readonly Error OverlappingQuantityRange = Error.Failure("PriceList.OverlappingQuantityRange", "Quantity range overlaps with an existing item for the same reference.");
    public static readonly Error OrderDiscountInvalidThreshold = Error.Failure("OrderDiscount.InvalidThreshold", "At least one threshold (MinOrderTotal or MinOrderQuantity) is required.");
    public static readonly Error OrderDiscountInvalidDiscount = Error.Failure("OrderDiscount.InvalidDiscount", "Either DiscountPercentage > 0 or DiscountAmount > 0 is required.");
    public static readonly Error OrderDiscountInvalidPercentage = Error.Failure("OrderDiscount.InvalidPercentage", "Discount percentage must be between 0 and 100.");
    public static Error OrderDiscountNotFound(Guid id) => Error.NotFound("OrderDiscount.NotFound", $"Order discount '{id}' not found.");

    // CustomerGroup errors
    public static readonly Error CustomerGroupNameRequired = Error.Failure("CustomerGroup.NameRequired", "Customer group name is required.");
    public static readonly Error CustomerGroupNotFound = Error.NotFound("CustomerGroup.NotFound", "Customer group not found.");
    public static readonly Error CustomerGroupAlreadyActive = Error.Failure("CustomerGroup.AlreadyActive", "Customer group is already active.");
    public static readonly Error CustomerGroupAlreadyInactive = Error.Failure("CustomerGroup.AlreadyInactive", "Customer group is already inactive.");
    public static readonly Error CustomerGroupMemberAlreadyExists = Error.Conflict("CustomerGroup.MemberAlreadyExists", "Customer is already a member of this group.");
    public static readonly Error CustomerGroupMemberNotFound = Error.NotFound("CustomerGroup.MemberNotFound", "Customer is not a member of this group.");

    // ScheduledPriceChange errors
    public static readonly Error ScheduledPriceChangeInvalidEffectiveAt = Error.Failure("ScheduledPriceChange.InvalidEffectiveAt", "EffectiveAt must be in the future.");
    public static readonly Error ScheduledPriceChangeAlreadyApplied = Error.Failure("ScheduledPriceChange.AlreadyApplied", "Scheduled price change has already been applied.");
    public static readonly Error ScheduledPriceChangeAlreadyCancelled = Error.Failure("ScheduledPriceChange.AlreadyCancelled", "Scheduled price change has already been cancelled.");
    public static readonly Error ScheduledPriceChangeNotFound = Error.NotFound("ScheduledPriceChange.NotFound", "Scheduled price change not found.");
    public static readonly Error ScheduledPriceChangeItemNotFound = Error.NotFound("ScheduledPriceChange.ItemNotFound", "Price list item referenced by the scheduled change was not found.");
}
