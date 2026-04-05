using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.RemovePromotionCondition;

internal sealed class RemovePromotionConditionCommandHandler(
    IPromotionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemovePromotionConditionCommand>
{
    public async Task<Result> Handle(RemovePromotionConditionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdWithDetailsAsync(request.PromotionId, cancellationToken);
        if (promotion is null) return Result.Failure(PromotionErrors.NotFound);

        var result = promotion.RemoveCondition(request.ConditionId);
        if (result.IsFailure) return result;

        repository.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
