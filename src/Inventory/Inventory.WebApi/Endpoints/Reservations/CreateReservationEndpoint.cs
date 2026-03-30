using Asp.Versioning;
using Inventory.Application.Reservations.Commands.CreateStockReservation;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Reservations;

internal sealed class CreateReservationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/reservations", async (
            CreateReservationRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateStockReservationCommand(
                request.ProductId, request.WarehouseId, request.LocationId, request.LotId,
                request.Quantity, request.SalesOrderId, request.ExpiresAt);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/v1/reservations/{result.Value}", new { id = result.Value })
                : result.ToProblem();
        })
        .WithName("CreateReservation")
        .WithTags("Reservations")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record CreateReservationRequest(
        Guid ProductId,
        Guid WarehouseId,
        Guid LocationId,
        Guid? LotId,
        decimal Quantity,
        Guid SalesOrderId,
        DateTimeOffset? ExpiresAt);
}
