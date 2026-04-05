using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class CustomerPriceListConfiguration : IEntityTypeConfiguration<CustomerPriceList>
{
    public void Configure(EntityTypeBuilder<CustomerPriceList> builder)
    {
        builder.ToTable("customer_price_lists");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(c => c.PriceListId).HasColumnName("price_list_id").IsRequired();
        builder.Property(c => c.ValidFrom).HasColumnName("valid_from");
        builder.Property(c => c.ValidTo).HasColumnName("valid_to");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(c => c.CustomerId).HasDatabaseName("ix_customer_price_lists_customer_id");
        builder.HasIndex(c => new { c.CustomerId, c.PriceListId })
            .HasDatabaseName("ix_customer_price_lists_customer_list");
    }
}
