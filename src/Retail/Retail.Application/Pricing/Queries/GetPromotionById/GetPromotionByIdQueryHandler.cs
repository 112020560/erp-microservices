using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Queries.GetPromotionById;

internal sealed class GetPromotionByIdQueryHandler(IPromotionRepository repository)
    : IQueryHandler<GetPromotionByIdQuery, PromotionDetailResponse>
{
    public async Task<Result<PromotionDetailResponse>> Handle(
        GetPromotionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (promotion is null) return Result.Failure<PromotionDetailResponse>(PromotionErrors.NotFound);

        var conditions = promotion.Conditions
            .Select(c => new PromotionConditionResponse(
                c.Id,
                c.ConditionType,
                c.DecimalValue,
                c.ReferenceId,
                c.IntValue))
            .ToList()
            .AsReadOnly() as IReadOnlyList<PromotionConditionResponse>;

        var actions = promotion.Actions
            .Select(a => new PromotionActionResponse(
                a.Id,
                a.ActionType,
                a.DiscountPercentage,
                a.DiscountAmount,
                a.TargetReferenceId,
                a.BuyQuantity,
                a.GetQuantity,
                a.BuyReferenceId,
                a.GetReferenceId))
            .ToList()
            .AsReadOnly() as IReadOnlyList<PromotionActionResponse>;

        var response = new PromotionDetailResponse(
            promotion.Id,
            promotion.Name,
            promotion.Description,
            promotion.CouponCode,
            promotion.IsAutomatic,
            promotion.IsActive,
            promotion.Priority,
            promotion.ValidFrom,
            promotion.ValidTo,
            promotion.MaxUses,
            promotion.MaxUsesPerCustomer,
            promotion.UsedCount,
            promotion.CanStackWithOthers,
            conditions!,
            actions!);

        return Result.Success(response);
    }
}
