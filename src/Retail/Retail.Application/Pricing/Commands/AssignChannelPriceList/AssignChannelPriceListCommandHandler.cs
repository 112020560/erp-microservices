using Retail.Application.Abstractions.Messaging;
using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;
using SharedKernel;

namespace Retail.Application.Pricing.Commands.AssignChannelPriceList;

internal sealed class AssignChannelPriceListCommandHandler(
    IPriceListRepository priceListRepository,
    IChannelPriceListRepository channelRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignChannelPriceListCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AssignChannelPriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = await priceListRepository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null) return Result.Failure<Guid>(PriceListErrors.NotFound);

        var existing = await channelRepository.GetByChannelAndListAsync(request.Channel, request.PriceListId, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(Error.Conflict("ChannelPriceList.AlreadyAssigned", "This price list is already assigned to the channel."));

        var result = ChannelPriceList.Create(request.Channel, request.PriceListId, request.Priority);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await channelRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
