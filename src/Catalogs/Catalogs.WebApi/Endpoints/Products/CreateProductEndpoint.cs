using Asp.Versioning;
using Catalogs.Application.Products.Commands.CreateProduct;
using Catalogs.WebApi.Extensions;
using MediatR;

namespace Catalogs.WebApi.Endpoints.Products;

internal sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (
            CreateProductRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductCommand(
                request.Sku,
                request.Name,
                request.Description,
                request.Price,
                request.Currency,
                request.CategoryId,
                request.BrandId);

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/v1/products/{result.Value}", new { id = result.Value })
                : result.ToProblem();
        })
        .WithName("CreateProduct")
        .WithTags("Products")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record CreateProductRequest(
        string Sku,
        string Name,
        string? Description,
        decimal Price,
        string Currency,
        Guid CategoryId,
        Guid BrandId);
}
