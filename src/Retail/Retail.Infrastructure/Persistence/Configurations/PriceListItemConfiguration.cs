using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class PriceListItemConfiguration : IEntityTypeConfiguration<PriceListItem>
{
    public void Configure(EntityTypeBuilder<PriceListItem> builder)
    {
        builder.ToTable("price_list_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.PriceListId).HasColumnName("price_list_id").IsRequired();
        builder.Property(i => i.ItemType).HasColumnName("item_type").IsRequired();
        builder.Property(i => i.ReferenceId).HasColumnName("reference_id");
        builder.Property(i => i.MinQuantity).HasColumnName("min_quantity").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(i => i.MaxQuantity).HasColumnName("max_quantity").HasColumnType("numeric(18,4)");
        builder.Property(i => i.Price).HasColumnName("price").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(i => i.DiscountPercentage).HasColumnName("discount_percentage").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(i => i.MinPrice).HasColumnName("min_price").HasColumnType("numeric(18,4)");
        builder.Property(i => i.PriceIncludesTax).HasColumnName("price_includes_tax").IsRequired();
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(i => new { i.PriceListId, i.ItemType, i.ReferenceId })
            .HasDatabaseName("ix_price_list_items_list_type_ref");
    }
}
