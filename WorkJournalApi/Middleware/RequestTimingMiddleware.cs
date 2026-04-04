using System.Diagnostics;

namespace WorkJournalApi.Middleware;

public sealed class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(
        RequestDelegate next,
        ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();

        await _next(context);

        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;

        _logger.LogInformation(
            "Request {Method} {Path} responded {StatusCode} in {ElapsedMs:0.00} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs);
    }
}
