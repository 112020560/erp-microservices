using Catalogs.Domain.Brands;

namespace Catalogs.Domain.Abstractions.Persistence;

public interface IBrandRepository
{
    Task<ProductBrand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductBrand>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    void Add(ProductBrand brand);
    void Update(ProductBrand brand);
}
