using System.Net;
using System.Text.Json;
using SpotMate.Application.DTOs;
using SpotMate.Application.DTOs.Responses;

namespace SpotMate.Web.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            
            var response = new ErrorResponse(statusCode.ToString(), exception.Message);
            var result = JsonSerializer.Serialize(response);
            
            await context.Response.WriteAsync(result);
        }
    }
}