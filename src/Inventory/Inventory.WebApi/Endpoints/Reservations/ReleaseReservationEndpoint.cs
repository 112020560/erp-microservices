using Asp.Versioning;
using Inventory.Application.Reservations.Commands.ReleaseStockReservation;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Reservations;

internal sealed class ReleaseReservationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/reservations/{id:guid}/release", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new ReleaseStockReservationCommand(id);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblem();
        })
        .WithName("ReleaseReservation")
        .WithTags("Reservations")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
