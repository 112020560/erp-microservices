using Retail.Domain.Pricing;
using Retail.Domain.Pricing.Abstractions;

namespace Retail.Application.Pricing.Services;

internal sealed class PriceListResolver(
    IPriceListRepository priceListRepository,
    IChannelPriceListRepository channelRepository,
    ICustomerPriceListRepository customerRepository,
    ICustomerGroupRepository customerGroupRepository)
{
    public async Task<(PriceList? List, string Source)> ResolveAsync(
        Guid? customerId,
        SalesChannel channel,
        DateTimeOffset at,
        bool includeDiscounts,
        CancellationToken cancellationToken)
    {
        // Priority 1: Customer-specific
        if (customerId.HasValue)
        {
            var customerAssignment = await customerRepository.GetActiveByCustomerAsync(customerId.Value, at, cancellationToken);
            if (customerAssignment is not null)
            {
                var list = await GetByIdAsync(customerAssignment.PriceListId, includeDiscounts, cancellationToken);
                if (list is not null && list.IsValidAt(at))
                    return (list, "Customer");
            }
        }

        // Priority 2: Customer Group (highest priority valid assignment among customer's groups)
        if (customerId.HasValue)
        {
            var groupAssignments = await customerGroupRepository
                .GetActiveGroupPriceListsForCustomerAsync(customerId.Value, at, cancellationToken);

            foreach (var assignment in groupAssignments.OrderByDescending(a => a.Priority))
            {
                var list = await GetByIdAsync(assignment.PriceListId, includeDiscounts, cancellationToken);
                if (list is not null && list.IsValidAt(at))
                    return (list, "CustomerGroup");
            }
        }

        // Priority 3: Channel-specific (highest priority one)
        var channelAssignments = await channelRepository.GetByChannelAsync(channel, cancellationToken);
        foreach (var assignment in channelAssignments.OrderByDescending(a => a.Priority))
        {
            var list = await GetByIdAsync(assignment.PriceListId, includeDiscounts, cancellationToken);
            if (list is not null && list.IsValidAt(at))
                return (list, "Channel");
        }

        // Priority 4: Default fallback (highest priority active list)
        var allLists = await priceListRepository.GetAllAsync(true, cancellationToken);
        var defaultList = allLists
            .Where(l => l.IsValidAt(at))
            .OrderByDescending(l => l.Priority)
            .FirstOrDefault();

        if (defaultList is not null)
        {
            var listWithItems = await GetByIdAsync(defaultList.Id, includeDiscounts, cancellationToken);
            if (listWithItems is not null)
                return (listWithItems, "Default");
        }

        return (null, string.Empty);
    }

    private Task<PriceList?> GetByIdAsync(Guid id, bool includeDiscounts, CancellationToken cancellationToken)
        => includeDiscounts
            ? priceListRepository.GetByIdWithItemsAndDiscountsAsync(id, cancellationToken)
            : priceListRepository.GetByIdWithItemsAsync(id, cancellationToken);

    public static (PriceListItem? Item, decimal FinalPrice) ResolveItemPrice(
        PriceList priceList,
        Guid productId,
        Guid? categoryId,
        decimal quantity)
    {
        var items = priceList.Items;

        var matched = FindBestMatch(items, PriceItemType.Product, productId, quantity)
                   ?? FindBestMatch(items, PriceItemType.Category, categoryId, quantity)
                   ?? FindBestMatch(items, PriceItemType.Default, null, quantity);

        if (matched is null) return (null, 0);

        var discounted = matched.Price * (1 - matched.DiscountPercentage / 100);
        var price = matched.MinPrice.HasValue
            ? Math.Max(discounted, matched.MinPrice.Value)
            : discounted;

        price = ApplyRounding(price, priceList.RoundingRule);

        return (matched, price);
    }

    public static PriceListItem? FindBestMatch(
        IReadOnlyList<PriceListItem> items,
        PriceItemType type,
        Guid? referenceId,
        decimal quantity)
    {
        return items
            .Where(i => i.ItemType == type &&
                        (type == PriceItemType.Default || i.ReferenceId == referenceId) &&
                        i.MinQuantity <= quantity &&
                        (!i.MaxQuantity.HasValue || quantity <= i.MaxQuantity.Value))
            .OrderByDescending(i => i.MinQuantity)
            .FirstOrDefault();
    }

    public static decimal ApplyRounding(decimal price, RoundingRule rule) => rule switch
    {
        RoundingRule.Nearest5  => Math.Round(price / 5, MidpointRounding.AwayFromZero) * 5,
        RoundingRule.Nearest10 => Math.Round(price / 10, MidpointRounding.AwayFromZero) * 10,
        RoundingRule.Ceil      => Math.Ceiling(price),
        RoundingRule.Floor     => Math.Floor(price),
        _                      => Math.Round(price, 2)
    };
}
