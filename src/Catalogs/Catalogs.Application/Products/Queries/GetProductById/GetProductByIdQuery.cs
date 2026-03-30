using Catalogs.Application.Abstractions.Messaging;

namespace Catalogs.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductResponse>;
