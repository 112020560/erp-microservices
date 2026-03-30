using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Brands;
using Microsoft.EntityFrameworkCore;

namespace Catalogs.Infrastructure.Persistence.Repositories;

internal sealed class BrandRepository(CatalogsDbContext context) : IBrandRepository
{
    public Task<ProductBrand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default) =>
        context.Brands.AnyAsync(
            b => b.Name.ToLower() == name.ToLower().Trim(),
            cancellationToken);

    public async Task<IReadOnlyList<ProductBrand>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        await context.Brands
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

    public void Add(ProductBrand brand) => context.Brands.Add(brand);

    public void Update(ProductBrand brand) => context.Brands.Update(brand);
}
