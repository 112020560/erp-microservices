using Inventory.Domain.PhysicalInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class CountLineConfiguration : IEntityTypeConfiguration<CountLine>
{
    public void Configure(EntityTypeBuilder<CountLine> builder)
    {
        builder.ToTable("count_lines");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.CountId).HasColumnName("count_id").IsRequired();
        builder.Property(l => l.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(l => l.LocationId).HasColumnName("location_id").IsRequired();
        builder.Property(l => l.LotId).HasColumnName("lot_id");
        builder.Property(l => l.SystemQuantity).HasColumnName("system_quantity").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(l => l.CountedQuantity).HasColumnName("counted_quantity").HasColumnType("numeric(18,4)");
        builder.Property(l => l.IsAdjusted).HasColumnName("is_adjusted").IsRequired();

        builder.Ignore(l => l.Difference);

        builder.HasIndex(l => l.CountId).HasDatabaseName("ix_count_lines_count_id");
    }
}
