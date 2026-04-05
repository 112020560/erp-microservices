using Retail.Application.Abstractions.Messaging;
using Retail.Application.Pricing.Services;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.ResolveCart;

internal sealed class ResolveCartQueryHandler(PriceListResolver resolver)
    : IQueryHandler<ResolveCartQuery, CartResolutionResponse>
{
    public async Task<Result<CartResolutionResponse>> Handle(
        ResolveCartQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var (priceList, source) = await resolver.ResolveAsync(
            request.CustomerId, request.Channel, now, includeDiscounts: true, cancellationToken);

        if (priceList is null)
            return Result.Failure<CartResolutionResponse>(
                Error.NotFound("Pricing.NoPriceListFound", "No active price list found for the given context."));

        // Resolve each item
        var resolvedItems = request.Items.Select(item =>
        {
            var (matchedItem, finalPrice) = PriceListResolver.ResolveItemPrice(
                priceList, item.ProductId, item.CategoryId, item.Quantity);

            var lineTotal = finalPrice * item.Quantity;

            return new CartItemResolution(
                item.ProductId,
                finalPrice,
                matchedItem?.DiscountPercentage ?? 0,
                lineTotal,
                matchedItem?.PriceIncludesTax ?? false,
                source);
        }).ToList().AsReadOnly() as IReadOnlyList<CartItemResolution>;

        // Calculate totals
        var subtotal = resolvedItems!.Sum(i => i.LineTotal);
        var totalQuantity = request.Items.Sum(i => i.Quantity);

        // Find best applicable order-level discount
        var bestDiscount = priceList.OrderDiscounts
            .Where(d => d.AppliesTo(subtotal, totalQuantity))
            .OrderByDescending(d => d.Priority)
            .FirstOrDefault();

        decimal discountAmount = 0;
        decimal? orderDiscountPercentage = null;

        if (bestDiscount is not null)
        {
            discountAmount = bestDiscount.CalculateDiscount(subtotal);
            orderDiscountPercentage = bestDiscount.DiscountPercentage > 0
                ? bestDiscount.DiscountPercentage
                : null;
        }

        var finalTotal = subtotal - discountAmount;

        return Result.Success(new CartResolutionResponse(
            resolvedItems!,
            subtotal,
            orderDiscountPercentage,
            discountAmount,
            finalTotal,
            priceList.Currency,
            priceList.Name));
    }
}
