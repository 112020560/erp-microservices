using Inventory.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class ProductSnapshotConfiguration : IEntityTypeConfiguration<ProductSnapshot>
{
    public void Configure(EntityTypeBuilder<ProductSnapshot> builder)
    {
        builder.ToTable("product_snapshots");

        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.ProductId).HasColumnName("product_id");
        builder.Property(p => p.Sku).HasColumnName("sku").HasMaxLength(50).IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.CategoryId).HasColumnName("category_id").IsRequired();
        builder.Property(p => p.BrandId).HasColumnName("brand_id").IsRequired();
        builder.Property(p => p.TrackingType).HasColumnName("tracking_type").IsRequired();
        builder.Property(p => p.MinimumStock).HasColumnName("minimum_stock").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(p => p.ReorderPoint).HasColumnName("reorder_point").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(p => p.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(p => p.LastSyncedAt).HasColumnName("last_synced_at").IsRequired();
    }
}
