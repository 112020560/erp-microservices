using Inventory.Domain.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Services;

internal sealed class MovementNumberGenerator(InventoryDbContext context) : IMovementNumberGenerator
{
    public async Task<string> GenerateAsync(string prefix, CancellationToken ct = default)
    {
        int year = DateTime.UtcNow.Year;
        int count = await context.InventoryMovements
            .CountAsync(m => m.MovementNumber.StartsWith($"{prefix}-{year}-"), ct);
        return $"{prefix}-{year}-{(count + 1):D5}";
    }
}
