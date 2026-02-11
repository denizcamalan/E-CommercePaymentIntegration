namespace ECommercePayment.Application.Services.RateLimit
{
    public interface IRateLimitService
    {
        Task<bool> IsRateLimitExceededAsync(string requesterId);
        Task IncrementRequestCountAsync(string requesterId);
        Task<T?> GetCachedResponseAsync<T>(string requesterId, string endpoint) where T : class;
        Task CacheResponseAsync<T>(string requesterId, string endpoint, T response, TimeSpan? expiration = null) where T : class;
    }
}
