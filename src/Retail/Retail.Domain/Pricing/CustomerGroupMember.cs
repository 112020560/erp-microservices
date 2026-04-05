namespace Retail.Domain.Pricing;

public sealed class CustomerGroupMember
{
    private CustomerGroupMember() { }

    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    internal static CustomerGroupMember Create(Guid groupId, Guid customerId)
    {
        return new CustomerGroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            CustomerId = customerId,
            AddedAt = DateTimeOffset.UtcNow
        };
    }
}
