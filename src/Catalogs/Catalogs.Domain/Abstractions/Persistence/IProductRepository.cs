using Catalogs.Domain.Products;

namespace Catalogs.Domain.Abstractions.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool? isActive,
        Guid? categoryId,
        Guid? brandId,
        CancellationToken cancellationToken = default);
    void Add(Product product);
    void Update(Product product);
}
