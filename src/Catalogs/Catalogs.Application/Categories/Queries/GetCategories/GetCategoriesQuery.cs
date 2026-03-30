using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IQuery<IReadOnlyList<CategoryResponse>>;
