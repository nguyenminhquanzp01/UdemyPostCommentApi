namespace Udemy.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Udemy.Domain.Data;
using StackExchange.Redis;

/// <summary>
/// Health check controller for Docker and monitoring.
/// </summary>
[ApiController]
[Route("")]
[AllowAnonymous]
public class HealthController(
    AppDbContext dbContext,
    IConnectionMultiplexer redis,
    ILogger<HealthController> logger) : ControllerBase
{
    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IConnectionMultiplexer _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    private readonly ILogger<HealthController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Basic health check endpoint.
    /// </summary>
    /// <returns>Health status.</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow });
    }

    /// <summary>
    /// Detailed health check endpoint with dependency status.
    /// </summary>
    /// <returns>Detailed health status.</returns>
    [HttpGet("health/detailed")]
    public async Task<IActionResult> HealthDetailed(CancellationToken cancellationToken = default)
    {
        var checks = new Dictionary<string, object>();

        // API status
        checks["api"] = "healthy";

        // Database check
        try
        {
            await _dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
            checks["database"] = new { status = "healthy", connectionString = "mysql" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            checks["database"] = new { status = "unhealthy", error = ex.Message };
        }

        // Redis check
        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync().ConfigureAwait(false);
            checks["redis"] = new { status = "healthy", connectionString = "redis:6379" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            checks["redis"] = new { status = "unhealthy", error = ex.Message };
        }

        // Overall status
        var allHealthy = checks.Values.All(c =>
            c is string s && s == "healthy" ||
            c is IDictionary<string, object> d && d.ContainsKey("status") && d["status"]?.ToString() == "healthy"
        );

        return allHealthy ? Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow, checks })
            : StatusCode(503, new { status = "unhealthy", timestamp = DateTimeOffset.UtcNow, checks });
    }

    /// <summary>
    /// Readiness check for Kubernetes and orchestration.
    /// </summary>
    /// <returns>Readiness status.</returns>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if we can connect to database
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);

            if (!canConnect)
            {
                return StatusCode(503, new { status = "not ready", reason = "Cannot connect to database" });
            }

            // Check Redis
            var db = _redis.GetDatabase();
            await db.PingAsync().ConfigureAwait(false);

            return Ok(new { status = "ready", timestamp = DateTimeOffset.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Readiness check failed");
            return StatusCode(503, new { status = "not ready", reason = ex.Message });
        }
    }

    /// <summary>
    /// Liveness check for container orchestration.
    /// </summary>
    /// <returns>Liveness status.</returns>
    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "alive", timestamp = DateTimeOffset.UtcNow });
    }
}
