using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.CreatePriceList;

internal sealed class CreatePriceListCommandHandler(
    IPriceListRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePriceListCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
    {
        var result = PriceList.Create(
            request.Name,
            request.Currency,
            request.Priority,
            request.RoundingRule,
            request.ValidFrom,
            request.ValidTo);

        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
