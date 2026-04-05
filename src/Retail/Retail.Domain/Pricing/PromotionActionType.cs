namespace Retail.Domain.Pricing;

public enum PromotionActionType
{
    CartPercentageDiscount = 1,    // % off entire subtotal
    CartFixedDiscount = 2,         // fixed amount off subtotal
    ProductPercentageDiscount = 3, // % off a specific product (ReferenceId = ProductId)
    ProductFixedDiscount = 4,      // fixed amount off a specific product (ReferenceId = ProductId)
    BuyXGetYFree = 5               // buy BuyQuantity of BuyReferenceId, get GetQuantity of GetReferenceId free
}
