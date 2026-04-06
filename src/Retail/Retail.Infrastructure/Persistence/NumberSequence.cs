namespace Retail.Infrastructure.Persistence;

internal sealed class NumberSequence
{
    public string SequenceName { get; set; } = string.Empty;
    public long CurrentValue { get; set; }
}
