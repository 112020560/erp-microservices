using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    Guid? ParentCategoryId) : ICommand<Guid>;
