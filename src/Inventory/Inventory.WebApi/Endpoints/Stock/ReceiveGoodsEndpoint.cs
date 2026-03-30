using Asp.Versioning;
using Inventory.Application.Stock.Commands.ReceiveGoods;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Stock;

internal sealed class ReceiveGoodsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock/receive", async (
            ReceiveGoodsRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new ReceiveGoodsCommand(
                request.WarehouseId,
                request.Reference,
                request.Notes,
                request.Date,
                request.Lines.Select(l => new ReceiveGoodsLineDto(l.ProductId, l.LocationId, l.LotId, l.Quantity, l.UnitCost)).ToList().AsReadOnly());

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { movementNumber = result.Value })
                : result.ToProblem();
        })
        .WithName("ReceiveGoods")
        .WithTags("Stock")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record ReceiveGoodsRequest(
        Guid WarehouseId,
        string? Reference,
        string? Notes,
        DateTimeOffset Date,
        IReadOnlyList<ReceiveGoodsLineRequest> Lines);

    internal sealed record ReceiveGoodsLineRequest(
        Guid ProductId,
        Guid LocationId,
        Guid? LotId,
        decimal Quantity,
        decimal UnitCost);
}
