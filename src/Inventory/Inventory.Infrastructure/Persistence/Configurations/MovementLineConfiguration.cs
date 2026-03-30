using Inventory.Domain.Movements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class MovementLineConfiguration : IEntityTypeConfiguration<MovementLine>
{
    public void Configure(EntityTypeBuilder<MovementLine> builder)
    {
        builder.ToTable("movement_lines");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.MovementId).HasColumnName("movement_id").IsRequired();
        builder.Property(l => l.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(l => l.SourceLocationId).HasColumnName("source_location_id").IsRequired();
        builder.Property(l => l.DestinationLocationId).HasColumnName("destination_location_id");
        builder.Property(l => l.LotId).HasColumnName("lot_id");
        builder.Property(l => l.Quantity).HasColumnName("quantity").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(l => l.UnitCost).HasColumnName("unit_cost").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(l => l.Notes).HasColumnName("notes").HasMaxLength(500);

        builder.HasIndex(l => l.MovementId).HasDatabaseName("ix_movement_lines_movement_id");
        builder.HasIndex(l => l.ProductId).HasDatabaseName("ix_movement_lines_product_id");
    }
}
