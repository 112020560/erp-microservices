using Microsoft.EntityFrameworkCore;
using Retail.Domain.Pricing;
using Retail.Domain.Sales;

namespace Retail.Infrastructure.Persistence;

public sealed class RetailDbContext(DbContextOptions<RetailDbContext> options) : DbContext(options)
{
    public DbSet<PriceList> PriceLists { get; set; }
    public DbSet<PriceListItem> PriceListItems { get; set; }
    public DbSet<ChannelPriceList> ChannelPriceLists { get; set; }
    public DbSet<CustomerPriceList> CustomerPriceLists { get; set; }
    public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();
    public DbSet<CustomerGroupMember> CustomerGroupMembers => Set<CustomerGroupMember>();
    public DbSet<CustomerGroupPriceList> CustomerGroupPriceLists => Set<CustomerGroupPriceList>();
    public DbSet<ScheduledPriceChange> ScheduledPriceChanges => Set<ScheduledPriceChange>();
    public DbSet<OrderVolumeDiscount> OrderVolumeDiscounts => Set<OrderVolumeDiscount>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionCondition> PromotionConditions => Set<PromotionCondition>();
    public DbSet<PromotionAction> PromotionActions => Set<PromotionAction>();
    public DbSet<PromotionUsage> PromotionUsages => Set<PromotionUsage>();

    // Sales
    public DbSet<SaleQuote> SaleQuotes => Set<SaleQuote>();
    public DbSet<SaleQuoteLine> SaleQuoteLines => Set<SaleQuoteLine>();
    public DbSet<AppliedPromotion> AppliedPromotions => Set<AppliedPromotion>();
    public DbSet<SaleInvoice> SaleInvoices => Set<SaleInvoice>();
    public DbSet<PaymentLine> PaymentLines => Set<PaymentLine>();
    internal DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailDbContext).Assembly);
    }
}
