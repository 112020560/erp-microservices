using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Retail.Infrastructure.Persistence;

internal sealed class RetailDbContextFactory : IDesignTimeDbContextFactory<RetailDbContext>
{
    public RetailDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RetailDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__RetailDb")
            ?? "Host=localhost;Port=5432;Database=retail_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(RetailDbContext).Assembly.FullName));

        return new RetailDbContext(optionsBuilder.Options);
    }
}
