using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.CreatePromotion;

internal sealed class CreatePromotionCommandHandler(
    IPromotionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePromotionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var result = Promotion.Create(
            request.Name,
            request.Description,
            request.CouponCode,
            request.ValidFrom,
            request.ValidTo,
            request.MaxUses,
            request.MaxUsesPerCustomer,
            request.Priority,
            request.CanStackWithOthers);

        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
