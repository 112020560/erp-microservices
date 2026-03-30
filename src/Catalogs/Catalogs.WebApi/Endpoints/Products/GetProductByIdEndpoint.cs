using Asp.Versioning;
using Catalogs.Application.Products.Queries.GetProductById;
using Catalogs.WebApi.Extensions;
using MediatR;

namespace Catalogs.WebApi.Endpoints.Products;

internal sealed class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
        .WithName("GetProductById")
        .WithTags("Products")
        .Produces<ProductResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
