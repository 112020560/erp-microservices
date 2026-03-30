using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalogs.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(CatalogsDbContext context) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
        context.Products.FirstOrDefaultAsync(
            p => p.Sku == Sku.FromPersistence(sku.ToUpperInvariant().Trim()),
            cancellationToken);

    public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
        context.Products.AnyAsync(
            p => p.Sku == Sku.FromPersistence(sku.ToUpperInvariant().Trim()),
            cancellationToken);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool? isActive,
        Guid? categoryId,
        Guid? brandId,
        CancellationToken cancellationToken = default)
    {
        var query = context.Products.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Product product) => context.Products.Add(product);

    public void Update(Product product) => context.Products.Update(product);
}
