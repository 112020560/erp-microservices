using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using SharedKernel;

namespace Catalogs.Application.Categories.Queries.GetCategories;

internal sealed class GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllActiveAsync(cancellationToken);

        var responses = categories
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description, c.ParentCategoryId, c.IsActive))
            .ToList();

        return Result.Success<IReadOnlyList<CategoryResponse>>(responses);
    }
}
