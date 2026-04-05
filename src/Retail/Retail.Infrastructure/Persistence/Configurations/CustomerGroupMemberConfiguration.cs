using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Retail.Domain.Pricing;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class CustomerGroupMemberConfiguration : IEntityTypeConfiguration<CustomerGroupMember>
{
    public void Configure(EntityTypeBuilder<CustomerGroupMember> builder)
    {
        builder.ToTable("customer_group_members");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.GroupId).HasColumnName("group_id").IsRequired();
        builder.Property(m => m.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(m => m.AddedAt).HasColumnName("added_at").IsRequired();

        builder.HasIndex(m => new { m.GroupId, m.CustomerId })
            .IsUnique()
            .HasDatabaseName("ix_customer_group_members_group_customer");
    }
}
