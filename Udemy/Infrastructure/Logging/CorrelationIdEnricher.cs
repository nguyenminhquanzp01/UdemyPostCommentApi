using System;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Udemy.Infrastructure.Logging;

/// <summary>
/// Enriches Serilog events with a CorrelationId retrieved from the current HttpContext.
/// </summary>
public sealed class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdEnricher"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
        if (propertyFactory == null) throw new ArgumentNullException(nameof(propertyFactory));

        try
        {
            var ctx = _httpContextAccessor.HttpContext;
            string? correlationId = null;

            if (ctx != null)
            {
                if (ctx.Items.TryGetValue("CorrelationId", out var item) && item != null)
                {
                    correlationId = item.ToString();
                }

                if (string.IsNullOrWhiteSpace(correlationId) && ctx.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValues))
                {
                    correlationId = headerValues.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            var prop = propertyFactory.CreateProperty("CorrelationId", correlationId);
            logEvent.AddPropertyIfAbsent(prop);
        }
        catch
        {
            // Enricher must not throw; swallow any enrichment errors.
        }
    }
}
