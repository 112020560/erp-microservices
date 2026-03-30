using Asp.Versioning;
using Catalogs.Application.Products.Commands.ActivateProduct;
using Catalogs.Application.Products.Commands.DeactivateProduct;
using Catalogs.WebApi.Extensions;
using MediatR;

namespace Catalogs.WebApi.Endpoints.Products;

internal sealed class ActivateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/products/{id:guid}/activate", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new ActivateProductCommand(id), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToProblem();
        })
        .WithName("ActivateProduct")
        .WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}

internal sealed class DeactivateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/products/{id:guid}/deactivate", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new DeactivateProductCommand(id), cancellationToken);
            return result.IsSuccess ? Results.NoContent() : result.ToProblem();
        })
        .WithName("DeactivateProduct")
        .WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
