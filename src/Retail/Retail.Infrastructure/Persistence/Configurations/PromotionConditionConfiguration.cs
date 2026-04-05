using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class PromotionConditionConfiguration : IEntityTypeConfiguration<PromotionCondition>
{
    public void Configure(EntityTypeBuilder<PromotionCondition> builder)
    {
        builder.ToTable("promotion_conditions");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.PromotionId).HasColumnName("promotion_id").IsRequired();
        builder.Property(c => c.ConditionType).HasColumnName("condition_type").IsRequired();
        builder.Property(c => c.DecimalValue).HasColumnName("decimal_value").HasColumnType("numeric(18,4)");
        builder.Property(c => c.ReferenceId).HasColumnName("reference_id");
        builder.Property(c => c.IntValue).HasColumnName("int_value");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(c => c.PromotionId)
            .HasDatabaseName("ix_promotion_conditions_promotion_id");
    }
}
