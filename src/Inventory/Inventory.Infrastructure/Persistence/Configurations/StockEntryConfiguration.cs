using Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
{
    public void Configure(EntityTypeBuilder<StockEntry> builder)
    {
        builder.ToTable("stock_entries");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(s => s.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(s => s.LocationId).HasColumnName("location_id").IsRequired();
        builder.Property(s => s.LotId).HasColumnName("lot_id");
        builder.Property(s => s.QuantityOnHand).HasColumnName("quantity_on_hand").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.QuantityReserved).HasColumnName("quantity_reserved").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.AverageCost).HasColumnName("average_cost").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.MinimumStock).HasColumnName("minimum_stock").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.ReorderPoint).HasColumnName("reorder_point").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Ignore(s => s.QuantityAvailable);
        builder.Ignore(s => s.IsLowStock);
        builder.Ignore(s => s.NeedsReorder);

        builder.HasIndex(s => new { s.ProductId, s.WarehouseId, s.LocationId, s.LotId })
            .IsUnique()
            .HasDatabaseName("ix_stock_entries_unique");

        builder.HasIndex(s => s.ProductId).HasDatabaseName("ix_stock_entries_product_id");
        builder.HasIndex(s => s.WarehouseId).HasDatabaseName("ix_stock_entries_warehouse_id");
    }
}
