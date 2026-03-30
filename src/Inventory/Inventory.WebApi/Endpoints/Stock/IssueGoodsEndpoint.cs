using Asp.Versioning;
using Inventory.Application.Stock.Commands.IssueGoods;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Stock;

internal sealed class IssueGoodsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock/issue", async (
            IssueGoodsRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new IssueGoodsCommand(
                request.WarehouseId,
                request.Reference,
                request.Notes,
                request.Date,
                request.Lines.Select(l => new IssueGoodsLineDto(l.ProductId, l.LocationId, l.LotId, l.Quantity)).ToList().AsReadOnly());

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { movementNumber = result.Value })
                : result.ToProblem();
        })
        .WithName("IssueGoods")
        .WithTags("Stock")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record IssueGoodsRequest(
        Guid WarehouseId,
        string? Reference,
        string? Notes,
        DateTimeOffset Date,
        IReadOnlyList<IssueGoodsLineRequest> Lines);

    internal sealed record IssueGoodsLineRequest(
        Guid ProductId,
        Guid LocationId,
        Guid? LotId,
        decimal Quantity);
}
