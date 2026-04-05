using Asp.Versioning;
using MediatR;
using Retail.Application.Pricing.Commands.RemoveGroupMember;
using Retail.WebApi.Endpoints;

namespace Retail.WebApi.Endpoints.Pricing;

internal sealed class RemoveGroupMemberEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/pricing/customer-groups/{id:guid}/members/{customerId:guid}", async (
            Guid id,
            Guid customerId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveGroupMemberCommand(id, customerId);
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error.Description);
        })
        .WithName("RemoveGroupMember")
        .WithTags("Pricing")
        .Produces(204)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
