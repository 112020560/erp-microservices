using Catalogs.Domain.Brands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalogs.Infrastructure.Persistence.Configurations;

internal sealed class ProductBrandConfiguration : IEntityTypeConfiguration<ProductBrand>
{
    public void Configure(EntityTypeBuilder<ProductBrand> builder)
    {
        builder.ToTable("brands");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id).HasColumnName("id");

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(b => b.Name)
            .IsUnique()
            .HasDatabaseName("ix_brands_name");

        builder.Property(b => b.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
