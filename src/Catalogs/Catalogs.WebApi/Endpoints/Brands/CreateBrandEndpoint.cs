using Asp.Versioning;
using Catalogs.Application.Brands.Commands.CreateBrand;
using Catalogs.Application.Brands.Queries.GetBrands;
using Catalogs.WebApi.Extensions;
using MediatR;

namespace Catalogs.WebApi.Endpoints.Brands;

internal sealed class CreateBrandEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/brands", async (
            CreateBrandRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new CreateBrandCommand(request.Name, request.Description),
                cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/v1/brands/{result.Value}", new { id = result.Value })
                : result.ToProblem();
        })
        .WithName("CreateBrand")
        .WithTags("Brands")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));

        app.MapGet("/brands", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetBrandsQuery(), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
        .WithName("GetBrands")
        .WithTags("Brands")
        .Produces<IReadOnlyList<BrandResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record CreateBrandRequest(string Name, string? Description);
}
