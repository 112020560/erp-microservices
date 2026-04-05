namespace Retail.Domain.Pricing;

public enum PromotionConditionType
{
    MinCartTotal = 1,       // cart subtotal >= DecimalValue
    MinCartQuantity = 2,    // total units >= DecimalValue
    ContainsProduct = 3,    // cart has product == ReferenceId
    ContainsCategory = 4,   // cart has product in category == ReferenceId
    CustomerInGroup = 5,    // customer belongs to group == ReferenceId
    MinItemCount = 6        // distinct product count >= IntValue
}
