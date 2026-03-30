using Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.ReservationNumber).HasColumnName("reservation_number").HasMaxLength(30).IsRequired();
        builder.Property(r => r.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(r => r.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(r => r.LocationId).HasColumnName("location_id").IsRequired();
        builder.Property(r => r.LotId).HasColumnName("lot_id");
        builder.Property(r => r.ReservedQuantity).HasColumnName("reserved_quantity").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(r => r.SalesOrderId).HasColumnName("sales_order_id").IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(r => r.ExpiresAt).HasColumnName("expires_at");

        builder.HasIndex(r => r.ReservationNumber).IsUnique().HasDatabaseName("ix_stock_reservations_number");
        builder.HasIndex(r => r.SalesOrderId).HasDatabaseName("ix_stock_reservations_sales_order_id");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_stock_reservations_status");
    }
}
