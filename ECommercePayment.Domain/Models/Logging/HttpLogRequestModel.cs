namespace ECommercePayment.Domain.Models.Logging
{
    public class HttpLogRequestModel
    {
        public string CorrelationId { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public string? QueryString { get; set; }
        public string IPAddress { get; set; }
        public string ContentType { get; set; }
        public Dictionary<string, object>? Headers { get; set; }
        public object? Body { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
