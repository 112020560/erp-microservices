using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class CustomerGroupPriceListConfiguration : IEntityTypeConfiguration<CustomerGroupPriceList>
{
    public void Configure(EntityTypeBuilder<CustomerGroupPriceList> builder)
    {
        builder.ToTable("customer_group_price_lists");
        builder.HasKey(gpl => gpl.Id);
        builder.Property(gpl => gpl.Id).HasColumnName("id");
        builder.Property(gpl => gpl.GroupId).HasColumnName("group_id").IsRequired();
        builder.Property(gpl => gpl.PriceListId).HasColumnName("price_list_id").IsRequired();
        builder.Property(gpl => gpl.Priority).HasColumnName("priority").IsRequired();
        builder.Property(gpl => gpl.ValidFrom).HasColumnName("valid_from");
        builder.Property(gpl => gpl.ValidTo).HasColumnName("valid_to");
        builder.Property(gpl => gpl.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(gpl => gpl.GroupId).HasDatabaseName("ix_customer_group_price_lists_group_id");
        builder.HasIndex(gpl => gpl.PriceListId).HasDatabaseName("ix_customer_group_price_lists_price_list_id");
    }
}
