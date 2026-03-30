using Inventory.Domain.Stock;

namespace Inventory.Domain.Abstractions.Persistence;

public interface IStockReservationRepository
{
    Task<StockReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockReservation>> GetBySalesOrderAsync(Guid salesOrderId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNumberAsync(string number, CancellationToken cancellationToken = default);
    void Add(StockReservation reservation);
    void Update(StockReservation reservation);
}
