using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Abstractions.Persistence;
using SharedKernel;

namespace Inventory.Application.Catalog.Queries.GetProductSnapshot;

internal sealed class GetProductSnapshotQueryHandler(
    IProductSnapshotRepository productSnapshotRepository)
    : IQueryHandler<GetProductSnapshotQuery, ProductSnapshotResponse>
{
    public async Task<Result<ProductSnapshotResponse>> Handle(
        GetProductSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = await productSnapshotRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (snapshot is null)
            return Result.Failure<ProductSnapshotResponse>(
                Error.NotFound("Product.NotFound", $"Product '{request.ProductId}' not found in inventory."));

        return Result.Success(new ProductSnapshotResponse(
            snapshot.ProductId,
            snapshot.Sku,
            snapshot.Name,
            snapshot.TrackingType.ToString(),
            snapshot.MinimumStock,
            snapshot.ReorderPoint,
            snapshot.IsActive));
    }
}
