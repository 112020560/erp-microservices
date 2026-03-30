using Inventory.Domain.PhysicalInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

internal sealed class PhysicalCountConfiguration : IEntityTypeConfiguration<PhysicalCount>
{
    public void Configure(EntityTypeBuilder<PhysicalCount> builder)
    {
        builder.ToTable("physical_counts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.CountNumber).HasColumnName("count_number").HasMaxLength(30).IsRequired();
        builder.Property(c => c.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(c => c.Status).HasColumnName("status").IsRequired();
        builder.Property(c => c.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(c => c.CompletedAt).HasColumnName("completed_at");
        builder.Property(c => c.Notes).HasColumnName("notes").HasMaxLength(1000);

        builder.HasIndex(c => c.CountNumber).IsUnique().HasDatabaseName("ix_physical_counts_number");
        builder.HasIndex(c => c.WarehouseId).HasDatabaseName("ix_physical_counts_warehouse_id");

        builder.HasMany(c => c.Lines)
            .WithOne()
            .HasForeignKey(l => l.CountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
