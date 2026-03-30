using Catalogs.Domain.Brands;
using Catalogs.Domain.Categories;
using Catalogs.Domain.Products;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalogs.Infrastructure.Persistence;

public sealed class CatalogsDbContext(DbContextOptions<CatalogsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> Categories { get; set; }
    public DbSet<ProductBrand> Brands { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogsDbContext).Assembly);

        // MassTransit Outbox tables
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
