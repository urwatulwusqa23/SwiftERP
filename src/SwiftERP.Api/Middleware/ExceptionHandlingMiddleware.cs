using System.Net;
using FluentValidation;
using SwiftERP.SharedKernel;

namespace SwiftERP.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation failed for {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Validation failed.",
                details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain rule violated for {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}
