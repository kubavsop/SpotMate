namespace SpotMate.Web.Middlewares;

public static class ExceptionHandlingMiddlewareExtension
{
    public static void UseExceptionHandlingMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}