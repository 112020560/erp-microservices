using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.GetVolumeTiers;

internal sealed class GetVolumeTiersQueryHandler(IPriceListRepository repository)
    : IQueryHandler<GetVolumeTiersQuery, IReadOnlyList<VolumeTierResponse>>
{
    public async Task<Result<IReadOnlyList<VolumeTierResponse>>> Handle(
        GetVolumeTiersQuery request,
        CancellationToken cancellationToken)
    {
        var priceList = await repository.GetByIdWithItemsAsync(request.PriceListId, cancellationToken);
        if (priceList is null)
            return Result.Failure<IReadOnlyList<VolumeTierResponse>>(PriceListErrors.NotFound);

        var tiers = priceList.Items
            .Where(i => i.ItemType == request.ItemType && i.ReferenceId == request.ReferenceId)
            .OrderBy(i => i.MinQuantity)
            .Select(i => new VolumeTierResponse(
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
            .AsReadOnly() as IReadOnlyList<VolumeTierResponse>;

        return Result.Success(tiers!);
    }
}
