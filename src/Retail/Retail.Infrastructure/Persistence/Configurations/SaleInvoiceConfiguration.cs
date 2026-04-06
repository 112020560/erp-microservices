using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Sales;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class SaleInvoiceConfiguration : IEntityTypeConfiguration<SaleInvoice>
{
    public void Configure(EntityTypeBuilder<SaleInvoice> builder)
    {
        builder.ToTable("sale_invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(50).IsRequired();
        builder.Property(i => i.QuoteId).HasColumnName("quote_id").IsRequired();
        builder.Property(i => i.CashierId).HasColumnName("cashier_id").IsRequired();
        builder.Property(i => i.RequiresElectronicInvoice).HasColumnName("requires_electronic_invoice").IsRequired();
        builder.Property(i => i.ElectronicDocumentId).HasColumnName("electronic_document_id");
        builder.Property(i => i.Total).HasColumnName("total").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();

        builder.HasMany(i => i.Payments)
            .WithOne()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
