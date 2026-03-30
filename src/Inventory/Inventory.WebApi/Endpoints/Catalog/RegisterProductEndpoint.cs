using Asp.Versioning;
using Inventory.Application.Catalog.Commands.RegisterProduct;
using Inventory.Domain.Catalog;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Catalog;

internal sealed class RegisterProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/register", async (
            RegisterProductRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterProductCommand(
                request.ProductId,
                request.TrackingType,
                request.MinimumStock,
                request.ReorderPoint);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblem();
        })
        .WithName("RegisterProduct")
        .WithTags("Catalog")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record RegisterProductRequest(
        Guid ProductId,
        TrackingType TrackingType,
        decimal MinimumStock,
        decimal ReorderPoint);
}
