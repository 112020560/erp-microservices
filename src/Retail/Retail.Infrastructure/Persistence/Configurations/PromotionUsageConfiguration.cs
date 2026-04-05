using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class PromotionUsageConfiguration : IEntityTypeConfiguration<PromotionUsage>
{
    public void Configure(EntityTypeBuilder<PromotionUsage> builder)
    {
        builder.ToTable("promotion_usages");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.PromotionId).HasColumnName("promotion_id").IsRequired();
        builder.Property(u => u.CustomerId).HasColumnName("customer_id");
        builder.Property(u => u.ExternalOrderId).HasColumnName("external_order_id").HasMaxLength(100);
        builder.Property(u => u.UsedAt).HasColumnName("used_at").IsRequired();

        builder.HasIndex(u => u.PromotionId)
            .HasDatabaseName("ix_promotion_usages_promotion_id");

        builder.HasIndex(u => u.CustomerId)
            .HasDatabaseName("ix_promotion_usages_customer_id");
    }
}
