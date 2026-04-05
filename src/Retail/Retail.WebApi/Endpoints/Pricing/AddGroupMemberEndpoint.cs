using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.AddGroupMember;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class AddGroupMemberEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/pricing/customer-groups/{id:guid}/members", async (
            Guid id,
            AddGroupMemberRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AddGroupMemberCommand(id, request.CustomerId);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/pricing/customer-groups/{id}/members/{result.Value}", new { Id = result.Value })
                : Results.Problem(result.Error.Description);
        })
        .WithName("AddGroupMember")
        .WithTags("Pricing")
        .Produces(201)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

public sealed record AddGroupMemberRequest(Guid CustomerId);
