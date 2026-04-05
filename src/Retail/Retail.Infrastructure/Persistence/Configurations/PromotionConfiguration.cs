using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("promotions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").HasColumnType("text");
        builder.Property(p => p.CouponCode).HasColumnName("coupon_code").HasMaxLength(50);
        builder.Property(p => p.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(p => p.ValidFrom).HasColumnName("valid_from").HasColumnType("timestamptz");
        builder.Property(p => p.ValidTo).HasColumnName("valid_to").HasColumnType("timestamptz");
        builder.Property(p => p.MaxUses).HasColumnName("max_uses");
        builder.Property(p => p.MaxUsesPerCustomer).HasColumnName("max_uses_per_customer");
        builder.Property(p => p.UsedCount).HasColumnName("used_count").IsRequired();
        builder.Property(p => p.Priority).HasColumnName("priority").IsRequired();
        builder.Property(p => p.CanStackWithOthers).HasColumnName("can_stack_with_others").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(p => new { p.IsActive, p.ValidFrom, p.ValidTo })
            .HasDatabaseName("ix_promotions_is_active_valid_from_valid_to");

        builder.HasIndex(p => p.CouponCode)
            .IsUnique()
            .HasFilter("coupon_code IS NOT NULL")
            .HasDatabaseName("ix_promotions_coupon_code_unique");

        builder.HasMany(p => p.Conditions)
            .WithOne()
            .HasForeignKey(c => c.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Conditions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Actions)
            .WithOne()
            .HasForeignKey(a => a.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Actions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Usages)
            .WithOne()
            .HasForeignKey(u => u.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Usages)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
