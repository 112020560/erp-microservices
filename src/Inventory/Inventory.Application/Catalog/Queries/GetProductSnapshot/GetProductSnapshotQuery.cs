using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Catalog.Queries.GetProductSnapshot;

public sealed record GetProductSnapshotQuery(Guid ProductId) : IQuery<ProductSnapshotResponse>;
