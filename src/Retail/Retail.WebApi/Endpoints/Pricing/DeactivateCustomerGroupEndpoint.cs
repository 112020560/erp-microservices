using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.DeactivateCustomerGroup;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class DeactivateCustomerGroupEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/pricing/customer-groups/{id:guid}/deactivate", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new DeactivateCustomerGroupCommand(id);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error.Description);
        })
        .WithName("DeactivateCustomerGroup")
        .WithTags("Pricing")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
