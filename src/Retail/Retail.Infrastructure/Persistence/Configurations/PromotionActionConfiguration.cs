using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class PromotionActionConfiguration : IEntityTypeConfiguration<PromotionAction>
{
    public void Configure(EntityTypeBuilder<PromotionAction> builder)
    {
        builder.ToTable("promotion_actions");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.PromotionId).HasColumnName("promotion_id").IsRequired();
        builder.Property(a => a.ActionType).HasColumnName("action_type").IsRequired();
        builder.Property(a => a.DiscountPercentage).HasColumnName("discount_percentage").HasColumnType("numeric(5,2)");
        builder.Property(a => a.DiscountAmount).HasColumnName("discount_amount").HasColumnType("numeric(18,4)");
        builder.Property(a => a.TargetReferenceId).HasColumnName("target_reference_id");
        builder.Property(a => a.BuyQuantity).HasColumnName("buy_quantity");
        builder.Property(a => a.GetQuantity).HasColumnName("get_quantity");
        builder.Property(a => a.BuyReferenceId).HasColumnName("buy_reference_id");
        builder.Property(a => a.GetReferenceId).HasColumnName("get_reference_id");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(a => a.PromotionId)
            .HasDatabaseName("ix_promotion_actions_promotion_id");
    }
}
