using MediatR;
using SharedKernel;

namespace Catalogs.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
