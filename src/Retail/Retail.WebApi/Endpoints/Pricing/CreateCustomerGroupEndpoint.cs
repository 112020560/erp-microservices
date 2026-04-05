using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.CreateCustomerGroup;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class CreateCustomerGroupEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/customer-groups", async (
            CreateCustomerGroupRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCustomerGroupCommand(request.Name, request.Description);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/customer-groups/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("CreateCustomerGroup")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record CreateCustomerGroupRequest(string Name, string? Description);
