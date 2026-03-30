using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.Infrastructure.Persistence;

internal sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=interchange.proxy.rlwy.net;Port=30299;Database=inventory_db;Username=postgres;Password=VIwCMnzKlshSsqCuFgcpzbkpXXqllyFu",
            npgsql => npgsql.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName));

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
