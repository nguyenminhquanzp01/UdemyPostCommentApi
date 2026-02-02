namespace Udemy.Infrastructure.Middleware;

/// <summary>
/// Middleware for enriching requests with correlation IDs for tracing.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        await _next(context).ConfigureAwait(false);
    }
}
