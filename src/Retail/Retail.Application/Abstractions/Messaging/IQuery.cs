using MediatR;
using SharedKernel;

namespace Retail.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
