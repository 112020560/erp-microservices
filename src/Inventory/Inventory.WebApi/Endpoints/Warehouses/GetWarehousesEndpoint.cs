using Asp.Versioning;
using Inventory.Application.Warehouses.Queries.GetWarehouses;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Warehouses;

internal sealed class GetWarehousesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/warehouses", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetWarehousesQuery();
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("GetWarehouses")
        .WithTags("Warehouses")
        .Produces<IReadOnlyList<WarehouseResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
