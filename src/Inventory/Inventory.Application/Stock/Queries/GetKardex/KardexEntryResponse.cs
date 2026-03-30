namespace Inventory.Application.Stock.Queries.GetKardex;

public sealed record KardexEntryResponse(
    Guid MovementId,
    string MovementNumber,
    string MovementType,
    DateTimeOffset Date,
    decimal QuantityIn,
    decimal QuantityOut,
    decimal Balance,
    decimal UnitCost,
    decimal TotalCost,
    decimal BalanceCost);
