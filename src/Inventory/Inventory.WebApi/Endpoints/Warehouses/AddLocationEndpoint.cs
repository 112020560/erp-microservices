using Asp.Versioning;
using Inventory.Application.Warehouses.Commands.AddLocation;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Warehouses;

internal sealed class AddLocationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/warehouses/{id:guid}/locations", async (
            Guid id,
            AddLocationRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AddLocationCommand(id, request.Aisle, request.Rack, request.Level, request.Name);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/v1/warehouses/{id}/locations/{result.Value}", new { id = result.Value })
                : result.ToProblem();
        })
        .WithName("AddLocation")
        .WithTags("Warehouses")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record AddLocationRequest(string Aisle, string Rack, string Level, string? Name);
}
