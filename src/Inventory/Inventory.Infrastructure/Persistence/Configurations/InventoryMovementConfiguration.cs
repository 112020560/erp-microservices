using Inventory.Domain.Movements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("inventory_movements");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.MovementNumber).HasColumnName("movement_number").HasMaxLength(30).IsRequired();
        builder.Property(m => m.MovementType).HasColumnName("movement_type").IsRequired();
        builder.Property(m => m.Status).HasColumnName("status").IsRequired();
        builder.Property(m => m.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(m => m.DestinationWarehouseId).HasColumnName("destination_warehouse_id");
        builder.Property(m => m.Reference).HasColumnName("reference").HasMaxLength(100);
        builder.Property(m => m.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(m => m.Date).HasColumnName("date").IsRequired();
        builder.Property(m => m.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(m => m.MovementNumber).IsUnique().HasDatabaseName("ix_inventory_movements_number");
        builder.HasIndex(m => m.WarehouseId).HasDatabaseName("ix_inventory_movements_warehouse_id");
        builder.HasIndex(m => m.Date).HasDatabaseName("ix_inventory_movements_date");
        builder.HasIndex(m => m.Status).HasDatabaseName("ix_inventory_movements_status");

        builder.HasMany(m => m.Lines)
            .WithOne()
            .HasForeignKey(l => l.MovementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(m => m.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
