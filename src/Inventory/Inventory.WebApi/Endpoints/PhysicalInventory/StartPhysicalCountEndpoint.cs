using Asp.Versioning;
using Inventory.Application.PhysicalInventory.Commands.StartPhysicalCount;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.PhysicalInventory;

internal sealed class StartPhysicalCountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/physical-counts", async (
            StartPhysicalCountRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new StartPhysicalCountCommand(
                request.WarehouseId,
                request.Notes,
                request.Lines.Select(l => new CountLineInitDto(l.ProductId, l.LocationId, l.LotId)).ToList().AsReadOnly());
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { countNumber = result.Value })
                : result.ToProblem();
        })
        .WithName("StartPhysicalCount")
        .WithTags("PhysicalInventory")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record StartPhysicalCountRequest(
        Guid WarehouseId,
        string? Notes,
        IReadOnlyList<CountLineInitRequest> Lines);

    internal sealed record CountLineInitRequest(Guid ProductId, Guid LocationId, Guid? LotId);
}
