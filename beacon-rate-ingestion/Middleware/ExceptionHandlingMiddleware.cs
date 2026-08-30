using Microsoft.AspNetCore.Mvc;

namespace DueDiligenceWorks.Beacon.RateIngestion.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "Request {Method} {Path} was cancelled by the client",
                context.Request.Method,
                context.Request.Path);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                logger.LogError(
                    exception,
                    "Unhandled exception after the response started for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }

            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        (int status, string title) = exception switch
        {
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "The request is invalid."),
            ArgumentException => (StatusCodes.Status400BadRequest, "The request is invalid."),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access is forbidden."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found."),
            TimeoutException => (StatusCodes.Status504GatewayTimeout, "A dependency timed out."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (status < StatusCodes.Status500InternalServerError)
        {
            logger.LogWarning(
                exception,
                "Request failed with status {StatusCode} for {Method} {Path}",
                status,
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://httpstatuses.com/{status}",
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(
            problem,
            CancellationToken.None);
    }
}