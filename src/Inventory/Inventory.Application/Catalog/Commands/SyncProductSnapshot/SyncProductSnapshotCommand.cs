using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.Catalog.Commands.SyncProductSnapshot;

public sealed record SyncProductSnapshotCommand(
    Guid ProductId,
    string Sku,
    string Name,
    Guid CategoryId,
    Guid BrandId) : ICommand;
