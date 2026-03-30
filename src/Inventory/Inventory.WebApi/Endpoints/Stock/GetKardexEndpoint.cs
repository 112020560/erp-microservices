using Asp.Versioning;
using Inventory.Application.Stock.Queries.GetKardex;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Stock;

internal sealed class GetKardexEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock/kardex/{productId:guid}", async (
            Guid productId,
            Guid? warehouseId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetKardexQuery(productId, warehouseId, from, to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("GetKardex")
        .WithTags("Stock")
        .Produces<IReadOnlyList<KardexEntryResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
