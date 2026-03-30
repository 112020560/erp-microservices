using Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
        builder.Property(w => w.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(w => w.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(w => w.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(w => w.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(w => w.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(w => w.Code).IsUnique().HasDatabaseName("ix_warehouses_code");
        builder.HasIndex(w => w.IsActive).HasDatabaseName("ix_warehouses_is_active");

        builder.HasMany(w => w.Locations)
            .WithOne()
            .HasForeignKey(l => l.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Locations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
