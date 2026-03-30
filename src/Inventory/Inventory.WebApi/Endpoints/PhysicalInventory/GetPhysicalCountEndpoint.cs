using Asp.Versioning;
using Inventory.Application.PhysicalInventory.Queries.GetPhysicalCount;
using Inventory.WebApi.Extensions;
using MediatR;

namespace Inventory.WebApi.Endpoints.PhysicalInventory;

internal sealed class GetPhysicalCountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/physical-counts/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPhysicalCountQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("GetPhysicalCount")
        .WithTags("PhysicalInventory")
        .Produces<PhysicalCountResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }
}
