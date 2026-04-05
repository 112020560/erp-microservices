using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class OrderVolumeDiscountConfiguration : IEntityTypeConfiguration<OrderVolumeDiscount>
{
    public void Configure(EntityTypeBuilder<OrderVolumeDiscount> builder)
    {
        builder.ToTable("order_volume_discounts");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.PriceListId).HasColumnName("price_list_id").IsRequired();
        builder.Property(d => d.MinOrderTotal).HasColumnName("min_order_total").HasColumnType("numeric(18,4)");
        builder.Property(d => d.MinOrderQuantity).HasColumnName("min_order_quantity").HasColumnType("numeric(18,4)");
        builder.Property(d => d.DiscountPercentage).HasColumnName("discount_percentage").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(d => d.DiscountAmount).HasColumnName("discount_amount").HasColumnType("numeric(18,4)");
        builder.Property(d => d.MaxDiscountAmount).HasColumnName("max_discount_amount").HasColumnType("numeric(18,4)");
        builder.Property(d => d.Priority).HasColumnName("priority").IsRequired();
        builder.Property(d => d.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(d => new { d.PriceListId, d.IsActive, d.Priority })
            .HasDatabaseName("ix_order_volume_discounts_list_active_priority");
    }
}
