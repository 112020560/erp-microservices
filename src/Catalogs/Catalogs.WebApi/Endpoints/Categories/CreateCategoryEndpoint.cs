using Asp.Versioning;
using Catalogs.Application.Categories.Commands.CreateCategory;
using Catalogs.Application.Categories.Queries.GetCategories;
using Catalogs.WebApi.Extensions;
using MediatR;

namespace Catalogs.WebApi.Endpoints.Categories;

internal sealed class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/categories", async (
            CreateCategoryRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new CreateCategoryCommand(request.Name, request.Description, request.ParentCategoryId),
                cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/v1/categories/{result.Value}", new { id = result.Value })
                : result.ToProblem();
        })
        .WithName("CreateCategory")
        .WithTags("Categories")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));

        app.MapGet("/categories", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetCategoriesQuery(), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
        .WithName("GetCategories")
        .WithTags("Categories")
        .Produces<IReadOnlyList<CategoryResponse>>()
        .WithOpenApi()
        .MapToApiVersion(new ApiVersion(1, 0));
    }

    internal sealed record CreateCategoryRequest(string Name, string? Description, Guid? ParentCategoryId);
}
