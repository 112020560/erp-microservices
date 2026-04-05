using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.UpdatePromotion;

internal sealed class UpdatePromotionCommandHandler(
    IPromotionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePromotionCommand>
{
    public async Task<Result> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (promotion is null) return Result.Failure(PromotionErrors.NotFound);

        var result = promotion.Update(
            request.Name,
            request.Description,
            request.ValidFrom,
            request.ValidTo,
            request.MaxUses,
            request.MaxUsesPerCustomer,
            request.Priority,
            request.CanStackWithOthers);

        if (result.IsFailure) return result;

        repository.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
