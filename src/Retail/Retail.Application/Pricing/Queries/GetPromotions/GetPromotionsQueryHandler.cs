using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.GetPromotions;

internal sealed class GetPromotionsQueryHandler(IPromotionRepository repository)
    : IQueryHandler<GetPromotionsQuery, IReadOnlyList<PromotionSummaryResponse>>
{
    public async Task<Result<IReadOnlyList<PromotionSummaryResponse>>> Handle(
        GetPromotionsQuery request,
        CancellationToken cancellationToken)
    {
        var promotions = await repository.GetAllAsync(request.IsActive, cancellationToken);

        var response = promotions
            .Select(p => new PromotionSummaryResponse(
                p.Id,
                p.Name,
                p.CouponCode,
                p.IsAutomatic,
                p.IsActive,
                p.Priority,
                p.ValidFrom,
                p.ValidTo,
                p.MaxUses,
                p.UsedCount,
                p.CanStackWithOthers,
                p.Conditions.Count,
                p.Actions.Count))
            .ToList()
            .AsReadOnly() as IReadOnlyList<PromotionSummaryResponse>;

        return Result.Success(response!);
    }
}
