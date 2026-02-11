namespace ECommercePayment.Domain.Models.Logging
{
    public class HttpLogResponseModel
    {
        public string CorrelationId { get; set; }
        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public Dictionary<string, object>? Headers { get; set; }
        public object? Body { get; set; }
        public DateTime Timestamp { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }
}
