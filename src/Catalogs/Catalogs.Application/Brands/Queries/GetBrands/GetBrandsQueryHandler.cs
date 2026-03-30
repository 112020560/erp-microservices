using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using SharedKernel;

namespace Catalogs.Application.Brands.Queries.GetBrands;

internal sealed class GetBrandsQueryHandler(IBrandRepository brandRepository)
    : IQueryHandler<GetBrandsQuery, IReadOnlyList<BrandResponse>>
{
    public async Task<Result<IReadOnlyList<BrandResponse>>> Handle(
        GetBrandsQuery request,
        CancellationToken cancellationToken)
    {
        var brands = await brandRepository.GetAllActiveAsync(cancellationToken);

        var responses = brands
            .Select(b => new BrandResponse(b.Id, b.Name, b.Description, b.IsActive))
            .ToList();

        return Result.Success<IReadOnlyList<BrandResponse>>(responses);
    }
}
