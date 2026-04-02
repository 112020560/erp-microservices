using Catalogs.Domain.Brands;
using Catalogs.Domain.Categories;
using Catalogs.Domain.Products;
using Catalogs.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Catalogs.Infrastructure.Persistence;

public sealed class CatalogsDbContext(DbContextOptions<CatalogsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> Categories { get; set; }
    public DbSet<ProductBrand> Brands { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogsDbContext).Assembly);

        modelBuilder.Entity<OutboxEvent>(b =>
        {
            b.ToTable("outbox_events");
            b.HasKey(e => e.Id);
            b.Property(e => e.EventType).IsRequired().HasMaxLength(256);
            b.Property(e => e.Payload).IsRequired();
            b.Property(e => e.Status).HasConversion<int>().IsRequired();
            b.HasIndex(e => new { e.Status, e.RetryCount, e.OccurredOn });
        });
    }
}
