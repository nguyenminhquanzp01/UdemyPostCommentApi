namespace Udemy.Infrastructure.Middleware;

/// <summary>
/// Middleware to add API version header to responses.
/// </summary>
public class ApiVersionHeaderMiddleware(RequestDelegate next, ILogger<ApiVersionHeaderMiddleware> logger)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<ApiVersionHeaderMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Add API version header to response
        context.Response.Headers["Api-Version"] = "1.0";
        context.Response.Headers["X-API-Version"] = "1.0";

        await _next(context).ConfigureAwait(false);
    }
}
