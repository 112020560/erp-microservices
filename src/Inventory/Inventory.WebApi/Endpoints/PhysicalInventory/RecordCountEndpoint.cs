using Asp.Versioning;
using Inventory.Application.PhysicalInventory.Commands.RecordPhysicalCount;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.PhysicalInventory;

internal sealed class RecordCountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/physical-counts/{id:guid}/lines/{lineId:guid}", async (
            Guid id,
            Guid lineId,
            RecordCountRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new RecordPhysicalCountCommand(id, lineId, request.CountedQuantity);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblem();
        })
        .WithName("RecordCount")
        .WithTags("PhysicalInventory")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record RecordCountRequest(decimal CountedQuantity);
}
