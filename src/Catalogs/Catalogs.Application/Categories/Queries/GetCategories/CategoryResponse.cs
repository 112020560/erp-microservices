namespace Catalogs.Application.Categories.Queries.GetCategories;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive);
