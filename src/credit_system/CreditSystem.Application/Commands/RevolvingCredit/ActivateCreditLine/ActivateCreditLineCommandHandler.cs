using CreditSystem.Domain.Abstractions.Projections;
using CreditSystem.Domain.Abstractions.Repositories;
using CreditSystem.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CreditSystem.Application.Commands.RevolvingCredit.ActivateCreditLine;

public class ActivateCreditLineCommandHandler : IRequestHandler<ActivateCreditLineCommand, ActivateCreditLineResponse>
{
    private readonly IRevolvingCreditRepository _repository;
    private readonly IProjectionEngine _projectionEngine;
    private readonly ILogger<ActivateCreditLineCommandHandler> _logger;

    public ActivateCreditLineCommandHandler(
        IRevolvingCreditRepository repository,
        IProjectionEngine projectionEngine,
        ILogger<ActivateCreditLineCommandHandler> logger)
    {
        _repository = repository;
        _projectionEngine = projectionEngine;
        _logger = logger;
    }

    public async Task<ActivateCreditLineResponse> Handle(
        ActivateCreditLineCommand request,
        CancellationToken cancellationToken)
    {
        var aggregate = await _repository.GetByIdAsync(request.CreditLineId, cancellationToken);

        if (aggregate == null)
        {
            _logger.LogWarning("Credit line {CreditLineId} not found", request.CreditLineId);
            return ActivateCreditLineResponse.Failed($"Credit line {request.CreditLineId} not found");
        }

        try
        {
            aggregate.Activate();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Cannot activate credit line {CreditLineId}: {Error}", 
                request.CreditLineId, ex.Message);
            return ActivateCreditLineResponse.Failed(ex.Message);
        }

        var events = aggregate.UncommittedEvents.ToList();
        await _repository.SaveAsync(aggregate, cancellationToken);

        foreach (var @event in events)
        {
            await _projectionEngine.ProjectEventAsync(@event, cancellationToken);
        }

        _logger.LogInformation("Credit line {CreditLineId} activated", request.CreditLineId);

        return ActivateCreditLineResponse.Activated(
            aggregate.Id,
            aggregate.State.ActivatedAt!.Value,
            aggregate.State.NextStatementDate!.Value);
    }
}