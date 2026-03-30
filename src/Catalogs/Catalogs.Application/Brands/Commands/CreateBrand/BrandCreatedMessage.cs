using SharedKernel.Contracts.Catalogs.Brands;

namespace Catalogs.Application.Brands.Commands.CreateBrand;

public sealed record BrandCreatedMessage : IBrandCreated
{
    public required Guid BrandId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
