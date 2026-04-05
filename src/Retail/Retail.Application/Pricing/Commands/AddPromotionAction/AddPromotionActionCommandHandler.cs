using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.AddPromotionAction;

internal sealed class AddPromotionActionCommandHandler(
    IPromotionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddPromotionActionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddPromotionActionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdWithDetailsAsync(request.PromotionId, cancellationToken);
        if (promotion is null) return Result.Failure<Guid>(PromotionErrors.NotFound);

        var result = promotion.AddAction(
            request.ActionType,
            request.DiscountPercentage,
            request.DiscountAmount,
            request.TargetReferenceId,
            request.BuyQuantity,
            request.GetQuantity,
            request.BuyReferenceId,
            request.GetReferenceId);

        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        repository.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
