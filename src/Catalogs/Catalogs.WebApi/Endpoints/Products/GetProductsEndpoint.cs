using Asp.Versioning;
using Catalogs.Application.Products.Queries.GetProducts;
using Catalogs.Application.Common;
using Catalogs.WebApi.Extensions;
using MediatR;

namespace Catalogs.WebApi.Endpoints.Products;

internal sealed class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
            IMediator mediator,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 20,
            bool? isActive = null,
            Guid? categoryId = null,
            Guid? brandId = null) =>
        {
            var query = new GetProductsQuery(page, pageSize, isActive, categoryId, brandId);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
        .WithName("GetProducts")
        .WithTags("Products")
        .Produces<PagedResult<ProductSummaryResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
