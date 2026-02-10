using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ECommercePayment.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private TimeSpan ExpireTime => TimeSpan.FromDays(1);

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
    {
        _cache = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _cache.StringSetAsync(key, json, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set cache for key {Key}", key);
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _cache.StringGetAsync(key);
            if (!value.HasValue)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cache for key {Key}", key);
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove cache for key {Key}", key);
        }
    }
}

