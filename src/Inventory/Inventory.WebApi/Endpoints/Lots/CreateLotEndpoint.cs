using Asp.Versioning;
using Inventory.Application.Lots.Commands.CreateLot;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.Lots;

internal sealed class CreateLotEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/lots", async (
            CreateLotRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateLotCommand(
                request.LotNumber, request.ProductId, request.ManufacturingDate, request.ExpirationDate);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/v1/lots/{result.Value}", new { id = result.Value })
                : result.ToProblem();
        })
        .WithName("CreateLot")
        .WithTags("Lots")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record CreateLotRequest(
        string LotNumber,
        Guid ProductId,
        DateOnly? ManufacturingDate,
        DateOnly? ExpirationDate);
}
