using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.AddPromotionCondition;

internal sealed class AddPromotionConditionCommandHandler(
    IPromotionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddPromotionConditionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddPromotionConditionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdWithDetailsAsync(request.PromotionId, cancellationToken);
        if (promotion is null) return Result.Failure<Guid>(PromotionErrors.NotFound);

        var result = promotion.AddCondition(
            request.ConditionType,
            request.DecimalValue,
            request.ReferenceId,
            request.IntValue);

        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        repository.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
