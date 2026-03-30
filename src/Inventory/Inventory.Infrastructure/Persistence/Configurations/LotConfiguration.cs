using Inventory.Domain.Lots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class LotConfiguration : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.ToTable("lots");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.LotNumber).HasColumnName("lot_number").HasMaxLength(100).IsRequired();
        builder.Property(l => l.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(l => l.ManufacturingDate).HasColumnName("manufacturing_date");
        builder.Property(l => l.ExpirationDate).HasColumnName("expiration_date");
        builder.Property(l => l.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(l => new { l.LotNumber, l.ProductId })
            .IsUnique()
            .HasDatabaseName("ix_lots_lotnumber_productid");

        builder.Ignore(l => l.IsExpired);
    }
}
