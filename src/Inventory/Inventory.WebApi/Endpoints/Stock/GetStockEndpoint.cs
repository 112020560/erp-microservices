using Asp.Versioning;
using Inventory.Application.Stock.Queries.GetStock;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Stock;

internal sealed class GetStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock", async (
            Guid? productId,
            Guid? warehouseId,
            bool? isLowStock,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStockQuery(productId, warehouseId, isLowStock);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("GetStock")
        .WithTags("Stock")
        .Produces<IReadOnlyList<StockEntryResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
