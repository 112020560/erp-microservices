using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Inventory.Application.Abstractions.Behaviors;

internal sealed class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogInformation("Inventory: handling {RequestName}", requestName);

        TResponse response = await next();

        if (response is Result { IsFailure: true } result)
            logger.LogWarning("Inventory: {RequestName} failed — {Error}", requestName, result.Error);

        return response;
    }
}
