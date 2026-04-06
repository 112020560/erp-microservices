using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Sales;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class AppliedPromotionConfiguration : IEntityTypeConfiguration<AppliedPromotion>
{
    public void Configure(EntityTypeBuilder<AppliedPromotion> builder)
    {
        builder.ToTable("applied_promotions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.QuoteId).HasColumnName("quote_id").IsRequired();
        builder.Property(p => p.PromotionId).HasColumnName("promotion_id").IsRequired();
        builder.Property(p => p.PromotionName).HasColumnName("promotion_name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.DiscountAmount).HasColumnName("discount_amount").HasColumnType("numeric(18,4)").IsRequired();

        builder.HasIndex(p => p.QuoteId).HasDatabaseName("ix_applied_promotions_quote_id");
    }
}
