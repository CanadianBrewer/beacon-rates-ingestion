using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace DueDiligenceWorks.Beacon.RateIngestion.Middleware;

public sealed class ApiKeyMiddleware(
    RequestDelegate next,
    ILogger<ApiKeyMiddleware> logger)
{
    private const string _apiKeyHeaderName = "x-ddw-apikey";
    private const string _expectedApiKey = "b584bc5c-f0f0-4ff4-9892-df42a2407c16";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(_apiKeyHeaderName, out StringValues values) ||
            values.Count != 1 ||
            !IsValidApiKey(values[0]))
        {
            logger.LogWarning(
                "Forbidden request with a missing or invalid API key for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteForbiddenResponseAsync(context);
            return;
        }

        await next(context);
    }

    private bool IsValidApiKey(string? suppliedApiKey)
    {
        if (string.IsNullOrEmpty(suppliedApiKey))
        {
            return false;
        }

        return string.Equals(suppliedApiKey, _expectedApiKey, StringComparison.OrdinalIgnoreCase);
    }

    private static Task WriteForbiddenResponseAsync(HttpContext context)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = "A valid API key is required.",
            Type = "https://httpstatuses.com/403",
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier
            }
        };

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problem, CancellationToken.None);
    }
}
