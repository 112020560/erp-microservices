using Asp.Versioning;
using Catalogs.Application.Products.Commands.UpdateProduct;
using Catalogs.WebApi.Extensions;
using MediatR;

namespace Catalogs.WebApi.Endpoints.Products;

internal sealed class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id:guid}", async (
            Guid id,
            UpdateProductRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProductCommand(id, request.Name, request.Description, request.CategoryId, request.BrandId);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess ? Results.NoContent() : result.ToProblem();
        })
        .WithName("UpdateProduct")
        .WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record UpdateProductRequest(
        string Name,
        string? Description,
        Guid CategoryId,
        Guid BrandId);
}
