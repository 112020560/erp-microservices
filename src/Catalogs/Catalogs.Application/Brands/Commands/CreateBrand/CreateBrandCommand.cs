using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand(string Name, string? Description) : ICommand<Guid>;
