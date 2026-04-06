using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Sales;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class SaleQuoteLineConfiguration : IEntityTypeConfiguration<SaleQuoteLine>
{
    public void Configure(EntityTypeBuilder<SaleQuoteLine> builder)
    {
        builder.ToTable("sale_quote_lines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.QuoteId).HasColumnName("quote_id").IsRequired();
        builder.Property(l => l.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(l => l.Sku).HasColumnName("sku").HasMaxLength(100).IsRequired();
        builder.Property(l => l.ProductName).HasColumnName("product_name").HasMaxLength(200).IsRequired();
        builder.Property(l => l.CategoryId).HasColumnName("category_id");
        builder.Property(l => l.Quantity).HasColumnName("quantity").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(l => l.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(l => l.DiscountPercentage).HasColumnName("discount_percentage").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(l => l.LineTotal).HasColumnName("line_total").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(l => l.PriceListName).HasColumnName("price_list_name").HasMaxLength(200);
        builder.Property(l => l.ResolutionSource).HasColumnName("resolution_source").HasMaxLength(100);

        builder.HasIndex(l => l.QuoteId).HasDatabaseName("ix_sale_quote_lines_quote_id");
    }
}
