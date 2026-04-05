using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.RemovePromotionAction;

internal sealed class RemovePromotionActionCommandHandler(
    IPromotionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemovePromotionActionCommand>
{
    public async Task<Result> Handle(RemovePromotionActionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdWithDetailsAsync(request.PromotionId, cancellationToken);
        if (promotion is null) return Result.Failure(PromotionErrors.NotFound);

        var result = promotion.RemoveAction(request.ActionId);
        if (result.IsFailure) return result;

        repository.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
