using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.DeactivatePromotion;

internal sealed class DeactivatePromotionCommandHandler(
    IPromotionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivatePromotionCommand>
{
    public async Task<Result> Handle(DeactivatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (promotion is null) return Result.Failure(PromotionErrors.NotFound);

        var result = promotion.Deactivate();
        if (result.IsFailure) return result;

        repository.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
