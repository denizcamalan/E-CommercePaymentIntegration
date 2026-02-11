using System.Net;

namespace ECommercePayment.Domain.DTOs.Response
{
    public class BaseErrorResponse
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string path { get; set; }
        public string title { get; set; }
        public DateTime timestamp { get; set; }
        public HttpStatusCode httpCode { get; set; }
        public string httpMessage { get; set; }
        public string message { get; set; }
        public string errorCode { get; set; }
    }
}
