namespace Inventory.Domain.Abstractions.Services;

public interface IMovementNumberGenerator
{
    Task<string> GenerateAsync(string prefix, CancellationToken ct = default);
}
