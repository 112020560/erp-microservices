using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using FacturaElectronica.Aplicacion.Wrappers;
using SharedKernel.Contracts.ElectronicInvoice;

namespace FacturaElectronica.Aplicacion.Behaviors;

internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : ResultadoFacturaElectronica
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogInformation("Processing request {RequestName}", requestName);

        TResponse result = await next();

        if (result.Exitoso)
        {
            logger.LogInformation("Completed request {RequestName}", requestName);
        }
        else
        {
            using (LogContext.PushProperty("Error", result.Mensaje, true))
            {
                logger.LogError("Completed request {RequestName} with error", requestName);
            }
        }

        return result;
    }
}