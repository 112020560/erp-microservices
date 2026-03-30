using Asp.Versioning;
using Inventory.Application.Stock.Commands.AdjustStock;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Stock;

internal sealed class AdjustStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock/adjust", async (
            AdjustStockRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new AdjustStockCommand(
                request.ProductId,
                request.WarehouseId,
                request.LocationId,
                request.LotId,
                request.NewQuantity,
                request.UnitCost,
                request.Reference,
                request.Notes);

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { movementNumber = result.Value })
                : result.ToProblem();
        })
        .WithName("AdjustStock")
        .WithTags("Stock")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record AdjustStockRequest(
        Guid ProductId,
        Guid WarehouseId,
        Guid LocationId,
        Guid? LotId,
        decimal NewQuantity,
        decimal UnitCost,
        string? Reference,
        string? Notes);
}
