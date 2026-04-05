using Asp.Versioning;
using Inventory.Application.Stock.Queries.SearchPosStock;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Stock;

internal sealed class SearchPosStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock/pos/search", async (
            string? q,
            string? sku,
            Guid? warehouseId,
            Guid? categoryId,
            bool onlyAvailable = false,
            int page = 1,
            int pageSize = 20,
            IMediator mediator = default!,
            CancellationToken cancellationToken = default) =>
        {
            var query = new SearchPosStockQuery(q, sku, warehouseId, categoryId, onlyAvailable, page, pageSize);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("SearchPosStock")
        .WithTags("Stock")
        .WithSummary("Buscar stock disponible desde punto de venta")
        .WithDescription("""
            Endpoint optimizado para consultas desde el POS.
            - `q`: búsqueda libre sobre nombre o SKU (parcial, sin distinción de mayúsculas)
            - `sku`: código exacto del producto (ideal para scanner de código de barras)
            - `warehouseId`: limitar resultados a una bodega específica
            - `categoryId`: filtrar por categoría de producto
            - `onlyAvailable`: retornar solo productos con stock > 0
            """)
        .Produces<PosStockPagedResponse>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
