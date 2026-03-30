namespace Inventory.Domain.Movements;

public enum MovementType
{
    GoodsReceipt = 1,
    GoodsIssue = 2,
    Transfer = 3,
    Adjustment = 4,
    PhysicalCountAdjust = 5,
    Shrinkage = 6,
    Return = 7
}

public enum MovementStatus { Draft = 0, Confirmed = 1, Cancelled = 2 }
