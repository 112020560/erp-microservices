using MediatR;
using SharedKernel;

namespace Inventory.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
