using CreditSystem.Domain.Aggregates.LoanContract.Events.Base;

namespace CreditSystem.Domain.Abstractions.Projections;

public interface IProjectionEngine
{
    Task ProjectEventAsync(IDomainEvent @event, CancellationToken ct = default);
    Task RebuildProjectionAsync(string projectionName, CancellationToken ct = default);
    Task RebuildAllProjectionsAsync(CancellationToken ct = default);
}