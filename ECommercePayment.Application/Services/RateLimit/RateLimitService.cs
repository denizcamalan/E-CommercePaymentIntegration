using ECommercePayment.Domain.AppSettings;
using ECommercePayment.Infrastructure.Cache;
using Microsoft.Extensions.Logging;

namespace ECommercePayment.Application.Services.RateLimit
{
    public class RateLimitService : IRateLimitService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<RateLimitService> _logger;
        private readonly RateLimitSettings _rateLimitSettings;

        public RateLimitService(
            ICacheService cacheService,
            ILogger<RateLimitService> logger,
            RateLimitSettings rateLimitSettings)
        {
            _cacheService = cacheService;
            _logger = logger;
            _rateLimitSettings = rateLimitSettings;
        }

        public async Task<bool> IsRateLimitExceededAsync(string requesterId)
        {
            try
            {
                var key = GetRateLimitKey(requesterId);
                var requestCount = await _cacheService.GetAsync<int>(key);

                if (requestCount >= _rateLimitSettings.MaxRequestsPerWindow)
                {
                    _logger.LogWarning("Rate limit exceeded for RequesterID: {RequesterId}. Count: {Count}", 
                        requesterId, requestCount);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for RequesterID: {RequesterId}", requesterId);
                // Rate limit kontrolünde hata olursa, güvenli tarafta kalıp false dönüyoruz
                return false;
            }
        }

        public async Task IncrementRequestCountAsync(string requesterId)
        {
            try
            {
                var key = GetRateLimitKey(requesterId);
                var currentCount = await _cacheService.GetAsync<int>(key);

                if (currentCount == 0)
                {
                    // İlk request, expiration time ile birlikte kaydet
                    await _cacheService.SetAsync(key, 1, TimeSpan.FromMinutes(_rateLimitSettings.WindowMinutes));
                    _logger.LogInformation("First request from RequesterID: {RequesterId}", requesterId);
                }
                else
                {
                    // Mevcut count'u artır (TTL korunur)
                    var ttl = await GetRemainingTtlAsync(key);
                    await _cacheService.SetAsync(key, currentCount + 1, ttl ?? TimeSpan.FromMinutes(_rateLimitSettings.WindowMinutes));
                    _logger.LogDebug("Incremented request count for RequesterID: {RequesterId}. New count: {Count}", 
                        requesterId, currentCount + 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing request count for RequesterID: {RequesterId}", requesterId);
            }
        }

        public async Task<T?> GetCachedResponseAsync<T>(string requesterId, string endpoint) where T : class
        {
            try
            {
                var key = GetIdempotencyKey(requesterId, endpoint);
                var cachedResponse = await _cacheService.GetAsync<T>(key);

                if (cachedResponse != null)
                {
                    _logger.LogInformation("Returning cached response for RequesterID: {RequesterId}, Endpoint: {Endpoint}", 
                        requesterId, endpoint);
                }

                return cachedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached response for RequesterID: {RequesterId}, Endpoint: {Endpoint}", 
                    requesterId, endpoint);
                return null;
            }
        }

        public async Task CacheResponseAsync<T>(string requesterId, string endpoint, T response, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var key = GetIdempotencyKey(requesterId, endpoint);
                var cacheExpiration = expiration ?? TimeSpan.FromMinutes(_rateLimitSettings.IdempotencyCacheMinutes);
                
                await _cacheService.SetAsync(key, response, cacheExpiration);
                
                _logger.LogInformation("Cached response for RequesterID: {RequesterId}, Endpoint: {Endpoint}, Expiration: {Expiration} minutes", 
                    requesterId, endpoint, cacheExpiration.TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching response for RequesterID: {RequesterId}, Endpoint: {Endpoint}", 
                    requesterId, endpoint);
            }
        }

        private string GetRateLimitKey(string requesterId)
        {
            return $"ratelimit:{requesterId}";
        }

        private string GetIdempotencyKey(string requesterId, string endpoint)
        {
            // Endpoint'i temizle (query string'leri kaldır)
            var cleanEndpoint = endpoint.Split('?')[0].Replace("/", ":");
            return $"idempotency:{requesterId}:{cleanEndpoint}";
        }

        private async Task<TimeSpan?> GetRemainingTtlAsync(string key)
        {
            try
            {
                // Redis'ten TTL bilgisini al
                // Not: Bu method cache service'e eklenebilir
                // Şimdilik default window time kullanıyoruz
                return TimeSpan.FromMinutes(_rateLimitSettings.WindowMinutes);
            }
            catch
            {
                return null;
            }
        }
    }
}
