using Catalogs.Domain.Categories;

namespace Catalogs.Domain.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    void Add(ProductCategory category);
    void Update(ProductCategory category);
}
