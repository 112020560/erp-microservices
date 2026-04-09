using Asp.Versioning;
using MediatR;
using Retail.Application.Abstractions.Services;
using Retail.Application.Sales.Queries.GetCustomerCreditStatus;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Sales;

internal sealed class GetCustomerCreditStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/sales/customers/{customerId}/credit-status",
            async (Guid customerId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    new GetCustomerCreditStatusQuery(customerId), cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Error.Description);
            })
            .WithName("GetCustomerCreditStatus")
            .WithTags("Sales")
            .WithSummary("Verificar estado de crédito de un cliente para el POS")
            .Produces<CustomerCreditStatusDto>(200)
            .WithOpenApi()
            .MapToApiVersion(new ApiVersion(1, 0));
    }
}
