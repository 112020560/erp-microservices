using Asp.Versioning;
using Inventory.Application.PhysicalInventory.Commands.ClosePhysicalCount;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.PhysicalInventory;

internal sealed class ClosePhysicalCountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/physical-counts/{id:guid}/close", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new ClosePhysicalCountCommand(id);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblem();
        })
        .WithName("ClosePhysicalCount")
        .WithTags("PhysicalInventory")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
