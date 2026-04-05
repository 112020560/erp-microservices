using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.GetPriceLists;

internal sealed class GetPriceListsQueryHandler(IPriceListRepository repository)
    : IQueryHandler<GetPriceListsQuery, IReadOnlyList<PriceListSummaryResponse>>
{
    public async Task<Result<IReadOnlyList<PriceListSummaryResponse>>> Handle(
        GetPriceListsQuery request,
        CancellationToken cancellationToken)
    {
        var lists = await repository.GetAllAsync(request.IsActive, cancellationToken);

        var response = lists
            .Select(pl => new PriceListSummaryResponse(
                pl.Id,
                pl.Name,
                pl.Currency,
                pl.Priority,
                pl.IsActive,
                pl.RoundingRule,
                pl.ValidFrom,
                pl.ValidTo,
                pl.Items.Count))
            .ToList()
            .AsReadOnly() as IReadOnlyList<PriceListSummaryResponse>;

        return Result.Success(response!);
    }
}
