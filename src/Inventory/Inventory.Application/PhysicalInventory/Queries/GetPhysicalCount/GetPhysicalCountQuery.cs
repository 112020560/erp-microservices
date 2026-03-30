using Inventory.Application.Abstractions.Messaging;

namespace Inventory.Application.PhysicalInventory.Queries.GetPhysicalCount;

public sealed record GetPhysicalCountQuery(Guid CountId) : IQuery<PhysicalCountResponse>;
