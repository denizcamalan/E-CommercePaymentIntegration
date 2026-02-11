namespace ECommercePayment.Domain.AppSettings
{
    public class RateLimitSettings
    {
        public int MaxRequestsPerWindow { get; set; }
        public int WindowMinutes { get; set; }
        public int IdempotencyCacheMinutes { get; set; }
    }
}
