using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Sales;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class SaleQuoteConfiguration : IEntityTypeConfiguration<SaleQuote>
{
    public void Configure(EntityTypeBuilder<SaleQuote> builder)
    {
        builder.ToTable("sale_quotes");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasColumnName("id");
        builder.Property(q => q.QuoteNumber).HasColumnName("quote_number").HasMaxLength(50).IsRequired();
        builder.Property(q => q.SalesPersonId).HasColumnName("sales_person_id").IsRequired();
        builder.Property(q => q.CustomerId).HasColumnName("customer_id");
        builder.Property(q => q.CustomerName).HasColumnName("customer_name").HasMaxLength(200).IsRequired();
        builder.Property(q => q.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(q => q.Channel).HasColumnName("channel").IsRequired();
        builder.Property(q => q.Status).HasColumnName("status").IsRequired();
        builder.Property(q => q.ValidUntil).HasColumnName("valid_until").HasColumnType("timestamptz").IsRequired();
        builder.Property(q => q.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(q => q.Notes).HasColumnName("notes").HasColumnType("text");
        builder.Property(q => q.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(q => q.VolumeDiscountAmount).HasColumnName("volume_discount_amount").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(q => q.PromotionDiscountAmount).HasColumnName("promotion_discount_amount").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(q => q.TaxAmount).HasColumnName("tax_amount").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(q => q.Total).HasColumnName("total").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(q => q.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(q => q.Status).HasDatabaseName("ix_sale_quotes_status");
        builder.HasIndex(q => q.SalesPersonId).HasDatabaseName("ix_sale_quotes_sales_person_id");
        builder.HasIndex(q => q.CustomerId).HasDatabaseName("ix_sale_quotes_customer_id");
        builder.HasIndex(q => q.ValidUntil).HasDatabaseName("ix_sale_quotes_valid_until");

        builder.HasMany(q => q.Lines)
            .WithOne()
            .HasForeignKey(l => l.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(q => q.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(q => q.AppliedPromotions)
            .WithOne()
            .HasForeignKey(p => p.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(q => q.AppliedPromotions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
