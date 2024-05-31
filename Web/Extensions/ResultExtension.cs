using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.Exceptions;
using SpotMate.Application.OperationResult;

namespace SpotMate.Web.Extensions;

public static class ResultExtension
{
    public static IActionResult ToIActionResult<TValue>(this Result<TValue> result)
    {
        return result.Match(
            onSuccess: SuccessResult,
            onFailure: FailureResult
        );
    }
    
    public static IActionResult ToIActionResult(this Result result)
    {
        return result.Match<IActionResult>(
            onSuccess: () => new OkResult(),
            onFailure: FailureResult
        );
    }
    
    public static IActionResult SuccessResult<TValue>(TValue value)
    {
        return new OkObjectResult(value);
    }
    
    public static ObjectResult FailureResult(Exception exception)
    {
        return exception switch
        {
            NotFoundException => new NotFoundObjectResult(exception.Message),
            BadRequestException => new BadRequestObjectResult(exception.Message),
            ForbiddenException => new ObjectResult(exception.Message) { StatusCode = 403 },
            IdentityException e => new BadRequestObjectResult(e.Errors),
            _ => new ObjectResult("Unexpected exception") { StatusCode = 500 }
        };
    }
}