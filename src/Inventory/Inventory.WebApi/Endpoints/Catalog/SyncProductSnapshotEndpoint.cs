using Asp.Versioning;
using Inventory.Application.Catalog.Commands.SyncProductSnapshot;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Catalog;

internal sealed class SyncProductSnapshotEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/sync", async (
            SyncProductSnapshotRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new SyncProductSnapshotCommand(
                request.ProductId,
                request.Sku,
                request.Name,
                request.CategoryId,
                request.BrandId);

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblem();
        })
        .WithName("SyncProductSnapshot")
        .WithTags("Catalog")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record SyncProductSnapshotRequest(
        Guid ProductId,
        string Sku,
        string Name,
        Guid CategoryId,
        Guid BrandId);
}
