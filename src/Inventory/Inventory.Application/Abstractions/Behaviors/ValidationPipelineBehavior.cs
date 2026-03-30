using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SharedKernel;

namespace Inventory.Application.Abstractions.Behaviors;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ValidationFailure[] failures = await ValidateAsync(request);

        if (failures.Length == 0)
            return await next();

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type resultType = typeof(TResponse).GetGenericArguments()[0];

            MethodInfo? failureMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod(nameof(Result<object>.ValidationFailure));

            if (failureMethod is not null)
                return (TResponse)failureMethod.Invoke(null, [CreateValidationError(failures)])!;
        }
        else if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(CreateValidationError(failures));
        }

        throw new ValidationException(failures);
    }

    private async Task<ValidationFailure[]> ValidateAsync(TRequest request)
    {
        if (!validators.Any())
            return [];

        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context)));

        return results
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToArray();
    }

    private static ValidationError CreateValidationError(ValidationFailure[] failures) =>
        new(failures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}
