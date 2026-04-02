using Catalogs.Application.Products.Commands.ActivateProduct;
using Catalogs.Application.Products.Commands.ChangePrice;
using Catalogs.Application.Products.Commands.CreateProduct;
using Catalogs.Application.Products.Commands.DeactivateProduct;
using Catalogs.Application.Products.Commands.UpdateProduct;
using Catalogs.Domain.Products.Events;
using System.Text.Json;

namespace Catalogs.Infrastructure.Messaging;

public static class CatalogsEventMapper
{
    private static readonly Dictionary<string, Func<string, object>> Map = new()
    {
        [nameof(ProductCreatedDomainEvent)] = payload =>
        {
            var e = JsonSerializer.Deserialize<ProductCreatedDomainEvent>(payload)!;
            return new ProductCreatedMessage
            {
                ProductId = e.ProductId,
                Sku = e.Sku,
                Name = e.Name,
                Description = e.Description,
                Price = e.Price,
                Currency = e.Currency,
                CategoryId = e.CategoryId,
                BrandId = e.BrandId,
                CreatedAt = e.CreatedAt
            };
        },
        [nameof(ProductUpdatedDomainEvent)] = payload =>
        {
            var e = JsonSerializer.Deserialize<ProductUpdatedDomainEvent>(payload)!;
            return new ProductUpdatedMessage
            {
                ProductId = e.ProductId,
                Name = e.Name,
                Description = e.Description,
                CategoryId = e.CategoryId,
                BrandId = e.BrandId,
                UpdatedAt = e.UpdatedAt
            };
        },
        [nameof(ProductPriceChangedDomainEvent)] = payload =>
        {
            var e = JsonSerializer.Deserialize<ProductPriceChangedDomainEvent>(payload)!;
            return new ProductPriceChangedMessage
            {
                ProductId = e.ProductId,
                Sku = e.Sku,
                OldPrice = e.OldPrice,
                OldCurrency = e.OldCurrency,
                NewPrice = e.NewPrice,
                NewCurrency = e.NewCurrency,
                ChangedAt = e.ChangedAt
            };
        },
        [nameof(ProductActivatedDomainEvent)] = payload =>
        {
            var e = JsonSerializer.Deserialize<ProductActivatedDomainEvent>(payload)!;
            return new ProductActivatedMessage
            {
                ProductId = e.ProductId,
                Sku = e.Sku,
                ActivatedAt = e.ActivatedAt
            };
        },
        [nameof(ProductDeactivatedDomainEvent)] = payload =>
        {
            var e = JsonSerializer.Deserialize<ProductDeactivatedDomainEvent>(payload)!;
            return new ProductDeactivatedMessage
            {
                ProductId = e.ProductId,
                Sku = e.Sku,
                DeactivatedAt = e.DeactivatedAt
            };
        }
    };

    public static object? ToMessage(string eventType, string payload) =>
        Map.TryGetValue(eventType, out var factory) ? factory(payload) : null;
}
