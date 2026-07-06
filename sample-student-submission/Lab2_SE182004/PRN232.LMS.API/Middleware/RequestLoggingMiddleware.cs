using System.Diagnostics;

namespace PRN232.LMS.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var watch = Stopwatch.StartNew();
        await _next(context);
        watch.Stop();
        var requestId = context.Items["X-Request-Id"]?.ToString() ?? "N/A";
        _logger.LogInformation("[{RequestId}] {Method} {Path} => {StatusCode} in {Elapsed}ms", requestId, context.Request.Method, context.Request.Path, context.Response.StatusCode, watch.ElapsedMilliseconds);
    }
}
