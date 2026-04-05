using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.UpdateCustomerGroup;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class UpdateCustomerGroupEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/pricing/customer-groups/{id:guid}", async (
            Guid id,
            UpdateCustomerGroupRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCustomerGroupCommand(id, request.Name, request.Description);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error.Description);
        })
        .WithName("UpdateCustomerGroup")
        .WithTags("Pricing")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record UpdateCustomerGroupRequest(string Name, string? Description);
