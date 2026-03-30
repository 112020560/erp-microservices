using Asp.Versioning;
using Inventory.Application.Warehouses.Commands.CreateWarehouse;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Warehouses;

internal sealed class CreateWarehouseEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/warehouses", async (
            CreateWarehouseRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateWarehouseCommand(request.Code, request.Name, request.Description);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/v1/warehouses/{result.Value}", new { id = result.Value })
                : result.ToProblem();
        })
        .WithName("CreateWarehouse")
        .WithTags("Warehouses")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record CreateWarehouseRequest(string Code, string Name, string? Description);
}
