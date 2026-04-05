using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.UpdatePriceList;

internal sealed class UpdatePriceListCommandHandler(
    IPriceListRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePriceListCommand>
{
    public async Task<Result> Handle(UpdatePriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (priceList is null) return Result.Failure(PriceListErrors.NotFound);

        var result = priceList.Update(request.Name, request.Priority, request.RoundingRule, request.ValidFrom, request.ValidTo);
        if (result.IsFailure) return result;

        repository.Update(priceList);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
