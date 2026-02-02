namespace Udemy.Application.Services;

using System.Text.Json;
using StackExchange.Redis;

/// <summary>
/// Redis cache service implementation.
/// </summary>
public class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IConnectionMultiplexer _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    private readonly ILogger<RedisCacheService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Gets a value from cache.
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key).ConfigureAwait(false);

            if (!value.HasValue)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache value for key: {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Sets a value in cache.
    /// </summary>
    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default, TimeSpan? expiration = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            await db.StringSetAsync(key, serialized, expiration).ConfigureAwait(false);
            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key).ConfigureAwait(false);
            _logger.LogDebug("Cache key removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {Key}", key);
        }
    }

    /// <summary>
    /// Checks if a key exists in cache.
    /// </summary>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache key existence: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Clears all cache.
    /// </summary>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            await server.FlushDatabaseAsync().ConfigureAwait(false);
            _logger.LogInformation("Cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }
}
