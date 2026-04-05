using Retail.Application.Abstractions.Messaging;
using Retail.Application.Pricing.Services;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.ResolvePrice;

internal sealed class ResolvePriceQueryHandler(PriceListResolver resolver)
    : IQueryHandler<ResolvePriceQuery, IReadOnlyList<ResolvedPriceResponse>>
{
    public async Task<Result<IReadOnlyList<ResolvedPriceResponse>>> Handle(
        ResolvePriceQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var (priceList, source) = await resolver.ResolveAsync(
            request.CustomerId, request.Channel, now, includeDiscounts: false, cancellationToken);

        if (priceList is null)
            return Result.Failure<IReadOnlyList<ResolvedPriceResponse>>(
                Error.NotFound("Pricing.NoPriceListFound", "No active price list found for the given context."));

        var responses = request.Items.Select(item =>
        {
            var (matchedItem, finalPrice) = PriceListResolver.ResolveItemPrice(
                priceList, item.ProductId, item.CategoryId, item.Quantity);

            return new ResolvedPriceResponse(
                item.ProductId,
                matchedItem?.Price ?? 0,
                matchedItem?.DiscountPercentage ?? 0,
                finalPrice,
                matchedItem?.PriceIncludesTax ?? false,
                priceList.Name,
                priceList.Currency,
                source);
        }).ToList().AsReadOnly() as IReadOnlyList<ResolvedPriceResponse>;

        return Result.Success(responses!);
    }
}
