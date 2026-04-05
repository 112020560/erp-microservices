using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.GetPriceListById;

internal sealed class GetPriceListByIdQueryHandler(IPriceListRepository repository)
    : IQueryHandler<GetPriceListByIdQuery, PriceListDetailResponse>
{
    public async Task<Result<PriceListDetailResponse>> Handle(
        GetPriceListByIdQuery request,
        CancellationToken cancellationToken)
    {
        var priceList = await repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (priceList is null) return Result.Failure<PriceListDetailResponse>(PriceListErrors.NotFound);

        var items = priceList.Items
            .Select(i => new PriceListItemResponse(
                i.Id,
                i.ItemType,
                i.ReferenceId,
                i.MinQuantity,
                i.MaxQuantity,
                i.Price,
                i.DiscountPercentage,
                i.MinPrice,
                i.PriceIncludesTax))
            .ToList()
            .AsReadOnly() as IReadOnlyList<PriceListItemResponse>;

        var response = new PriceListDetailResponse(
            priceList.Id,
            priceList.Name,
            priceList.Currency,
            priceList.Priority,
            priceList.IsActive,
            priceList.RoundingRule,
            priceList.ValidFrom,
            priceList.ValidTo,
            items!);

        return Result.Success(response);
    }
}
