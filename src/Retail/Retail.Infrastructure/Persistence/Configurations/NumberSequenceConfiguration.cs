using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Retail.Infrastructure.Persistence.Configurations;

internal sealed class NumberSequenceConfiguration : IEntityTypeConfiguration<NumberSequence>
{
    public void Configure(EntityTypeBuilder<NumberSequence> builder)
    {
        builder.ToTable("number_sequences");
        builder.HasKey(n => n.SequenceName);
        builder.Property(n => n.SequenceName).HasColumnName("sequence_name").HasMaxLength(50);
        builder.Property(n => n.CurrentValue).HasColumnName("current_value").HasColumnType("bigint");
    }
}
