using Inventory.Application.Abstractions.Messaging;
using Inventory.Domain.Catalog;

namespace Inventory.Application.Catalog.Commands.RegisterProduct;

public sealed record RegisterProductCommand(
    Guid ProductId,
    TrackingType TrackingType,
    decimal MinimumStock,
    decimal ReorderPoint) : ICommand;
