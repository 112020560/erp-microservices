using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class ScheduledPriceChangeConfiguration : IEntityTypeConfiguration<ScheduledPriceChange>
{
    public void Configure(EntityTypeBuilder<ScheduledPriceChange> builder)
    {
        builder.ToTable("scheduled_price_changes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.PriceListId).HasColumnName("price_list_id").IsRequired();
        builder.Property(c => c.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(c => c.NewPrice).HasColumnName("new_price").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(c => c.NewDiscountPercentage).HasColumnName("new_discount_percentage").HasColumnType("numeric(5,2)");
        builder.Property(c => c.NewMinPrice).HasColumnName("new_min_price").HasColumnType("numeric(18,4)");
        builder.Property(c => c.EffectiveAt).HasColumnName("effective_at").IsRequired();
        builder.Property(c => c.Status).HasColumnName("status").IsRequired();
        builder.Property(c => c.AppliedAt).HasColumnName("applied_at");
        builder.Property(c => c.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(c => new { c.Status, c.EffectiveAt })
            .HasDatabaseName("ix_scheduled_price_changes_status_effective_at");
        builder.HasIndex(c => c.PriceListId)
            .HasDatabaseName("ix_scheduled_price_changes_price_list_id");
    }
}
