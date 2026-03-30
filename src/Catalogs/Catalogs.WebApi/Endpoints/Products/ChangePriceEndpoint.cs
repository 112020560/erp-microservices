using Asp.Versioning;
using Catalogs.Application.Products.Commands.ChangePrice;
using Catalogs.WebApi.Extensions;
using MediatR;

namespace Catalogs.WebApi.Endpoints.Products;

internal sealed class ChangePriceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/products/{id:guid}/price", async (
            Guid id,
            ChangePriceRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new ChangePriceCommand(id, request.NewPrice, request.Currency), cancellationToken);

            return result.IsSuccess ? Results.NoContent() : result.ToProblem();
        })
        .WithName("ChangeProductPrice")
        .WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record ChangePriceRequest(decimal NewPrice, string Currency);
}
