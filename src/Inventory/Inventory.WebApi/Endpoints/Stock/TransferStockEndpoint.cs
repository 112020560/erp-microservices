using Asp.Versioning;
using Inventory.Application.Stock.Commands.TransferStock;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Stock;

internal sealed class TransferStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock/transfer", async (
            TransferStockRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new TransferStockCommand(
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                request.Reference,
                request.Notes,
                request.Date,
                request.Lines.Select(l => new TransferLineDto(
                    l.ProductId, l.SourceLocationId, l.DestinationLocationId, l.LotId, l.Quantity)).ToList().AsReadOnly());

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { movementNumber = result.Value })
                : result.ToProblem();
        })
        .WithName("TransferStock")
        .WithTags("Stock")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record TransferStockRequest(
        Guid SourceWarehouseId,
        Guid DestinationWarehouseId,
        string? Reference,
        string? Notes,
        DateTimeOffset Date,
        IReadOnlyList<TransferLineRequest> Lines);

    internal sealed record TransferLineRequest(
        Guid ProductId,
        Guid SourceLocationId,
        Guid DestinationLocationId,
        Guid? LotId,
        decimal Quantity);
}
