using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.ToTable("price_lists");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(p => p.Priority).HasColumnName("priority").IsRequired();
        builder.Property(p => p.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(p => p.RoundingRule).HasColumnName("rounding_rule").IsRequired();
        builder.Property(p => p.ValidFrom).HasColumnName("valid_from");
        builder.Property(p => p.ValidTo).HasColumnName("valid_to");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(p => p.IsActive).HasDatabaseName("ix_price_lists_is_active");
        builder.HasIndex(p => p.Priority).HasDatabaseName("ix_price_lists_priority");

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.PriceListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.OrderDiscounts)
            .WithOne()
            .HasForeignKey(d => d.PriceListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.OrderDiscounts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
