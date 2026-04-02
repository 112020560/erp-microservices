using Inventory.Domain.Catalog;
using Inventory.Domain.Lots;
using Inventory.Domain.Movements;
using Inventory.Domain.PhysicalInventory;
using Inventory.Domain.Stock;
using Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ProductSnapshot> ProductSnapshots { get; set; }
    public DbSet<Lot> Lots { get; set; }
    public DbSet<StockEntry> StockEntries { get; set; }
    public DbSet<StockReservation> StockReservations { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }
    public DbSet<MovementLine> MovementLines { get; set; }
    public DbSet<PhysicalCount> PhysicalCounts { get; set; }
    public DbSet<CountLine> CountLines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
