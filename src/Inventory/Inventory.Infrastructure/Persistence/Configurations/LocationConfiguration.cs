using Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("warehouse_locations");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(l => l.Aisle).HasColumnName("aisle").HasMaxLength(20).IsRequired();
        builder.Property(l => l.Rack).HasColumnName("rack").HasMaxLength(20).IsRequired();
        builder.Property(l => l.Level).HasColumnName("level").HasMaxLength(20).IsRequired();
        builder.Property(l => l.Name).HasColumnName("name").HasMaxLength(100);
        builder.Property(l => l.IsActive).HasColumnName("is_active").IsRequired();

        builder.Ignore(l => l.Code);

        builder.HasIndex(l => l.WarehouseId).HasDatabaseName("ix_warehouse_locations_warehouse_id");
        builder.HasIndex(l => new { l.WarehouseId, l.Aisle, l.Rack, l.Level })
            .IsUnique()
            .HasDatabaseName("ix_warehouse_locations_unique");
    }
}
