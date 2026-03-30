using SharedKernel.Contracts.Catalogs.Categories;

namespace Catalogs.Application.Categories.Commands.CreateCategory;

public sealed record CategoryCreatedMessage : ICategoryCreated
{
    public required Guid CategoryId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
