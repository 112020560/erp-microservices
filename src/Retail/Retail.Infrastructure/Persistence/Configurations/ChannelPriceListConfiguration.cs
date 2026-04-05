using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class ChannelPriceListConfiguration : IEntityTypeConfiguration<ChannelPriceList>
{
    public void Configure(EntityTypeBuilder<ChannelPriceList> builder)
    {
        builder.ToTable("channel_price_lists");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.Channel).HasColumnName("channel").IsRequired();
        builder.Property(c => c.PriceListId).HasColumnName("price_list_id").IsRequired();
        builder.Property(c => c.Priority).HasColumnName("priority").IsRequired();
        builder.Property(c => c.ValidFrom).HasColumnName("valid_from");
        builder.Property(c => c.ValidTo).HasColumnName("valid_to");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(c => c.Channel).HasDatabaseName("ix_channel_price_lists_channel");
        builder.HasIndex(c => new { c.Channel, c.PriceListId })
            .IsUnique()
            .HasDatabaseName("ix_channel_price_lists_channel_list");
    }
}
