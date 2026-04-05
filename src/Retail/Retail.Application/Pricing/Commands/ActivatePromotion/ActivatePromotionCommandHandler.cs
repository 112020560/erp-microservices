using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.ActivatePromotion;

internal sealed class ActivatePromotionCommandHandler(
    IPromotionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ActivatePromotionCommand>
{
    public async Task<Result> Handle(ActivatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (promotion is null) return Result.Failure(PromotionErrors.NotFound);

        var result = promotion.Activate();
        if (result.IsFailure) return result;

        repository.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
