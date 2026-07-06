using System.Diagnostics;

namespace PRN232.LMS.API.Middleware;

public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string RequestIdHeaderName = "X-Request-Id";

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Check if the header exists
        if (!context.Request.Headers.TryGetValue(RequestIdHeaderName, out var requestId))
        {
            // 2. Generate a new GUID if not provided
            requestId = Guid.NewGuid().ToString();
            context.Request.Headers.Add(RequestIdHeaderName, requestId);
        }

        // Add to HttpContext.Items for later use
        context.Items[RequestIdHeaderName] = requestId.ToString();

        // 3. Add to Response Headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(RequestIdHeaderName))
            {
                context.Response.Headers.Add(RequestIdHeaderName, requestId);
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
