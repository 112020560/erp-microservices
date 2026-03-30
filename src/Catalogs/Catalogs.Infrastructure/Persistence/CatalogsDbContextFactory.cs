using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalogs.Infrastructure.Persistence;

internal sealed class CatalogsDbContextFactory : IDesignTimeDbContextFactory<CatalogsDbContext>
{
    public CatalogsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogsDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=interchange.proxy.rlwy.net;Database=catalog_db;Username=postgres;Password=VIwCMnzKlshSsqCuFgcpzbkpXXqllyFu;Port=30299",
            npgsql => npgsql.MigrationsAssembly(typeof(CatalogsDbContext).Assembly.FullName));

        return new CatalogsDbContext(optionsBuilder.Options);
    }
}
