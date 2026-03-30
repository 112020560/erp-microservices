using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace Catalogs.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(CatalogsDbContext context) : ICategoryRepository
{
    public Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default) =>
        context.Categories.AnyAsync(
            c => c.Name.ToLower() == name.ToLower().Trim(),
            cancellationToken);

    public async Task<IReadOnlyList<ProductCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        await context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public void Add(ProductCategory category) => context.Categories.Add(category);

    public void Update(ProductCategory category) => context.Categories.Update(category);
}
