using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FacturaElectronica.Infraestructura.Adapters.Outbound.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__electronic_invoice_db")
            ?? "Host=localhost;Port=5432;Database=electronic_invoice_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(
            connectionString,
            o => o.MigrationsHistoryTable("__ef_migrations_history", "invoicing"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
