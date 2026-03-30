using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using SharedKernel.Enums;

namespace Inventory.WebApi.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert a successful result to a problem.");

        return MapError(result.Error);
    }

    public static IResult ToProblem<T>(this Result<T> result) =>
        result.IsSuccess
            ? throw new InvalidOperationException("Cannot convert a successful result to a problem.")
            : MapError(result.Error);

    private static IResult MapError(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            detail: error.Description,
            title: error.Code,
            statusCode: statusCode);
    }
}
