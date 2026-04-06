using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Sales;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class PaymentLineConfiguration : IEntityTypeConfiguration<PaymentLine>
{
    public void Configure(EntityTypeBuilder<PaymentLine> builder)
    {
        builder.ToTable("payment_lines");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.InvoiceId).HasColumnName("invoice_id").IsRequired();
        builder.Property(p => p.Method).HasColumnName("method").IsRequired();
        builder.Property(p => p.Amount).HasColumnName("amount").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(p => p.Reference).HasColumnName("reference").HasMaxLength(200);

        builder.HasIndex(p => p.InvoiceId).HasDatabaseName("ix_payment_lines_invoice_id");
    }
}
