using Microsoft.AspNetCore.Mvc;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.OperationResult;

namespace SpotMate.Web.Extensions;

public static class ResultExtension
{
    public static ActionResult<TValue> ToIActionResult<TValue>(this Result<TValue> result)
    {
        return result.Match(
            onSuccess: SuccessResult,
            onFailure: FailureActionResult<TValue>
        );
    }
    
    public static IActionResult ToIActionResult(this Result result)
    {
        return result.Match<IActionResult>(
            onSuccess: () => new NoContentResult(),
            onFailure: FailureResult
        );
    }
    
    private static ActionResult<TValue> SuccessResult<TValue>(TValue value)
    {
        return new OkObjectResult(value);
    }

    private static ObjectResult FailureResult(Exception exception)
    {
        return exception switch
        {
            NotFoundException => new NotFoundObjectResult(new ErrorResponse(exception.Message)),
            BadRequestException => new BadRequestObjectResult(new ErrorResponse(exception.Message)),
            ForbiddenException => new ObjectResult(new ErrorResponse(exception.Message)) { StatusCode = 403 },
            IdentityException e => new BadRequestObjectResult(e.Errors),
            _ => new ObjectResult(new ErrorResponse("Unexpected exception")) { StatusCode = 500 }
        };
    }

    private static ActionResult<TValue> FailureActionResult<TValue>(Exception exception)
    {
        return FailureResult(exception);
    }
}