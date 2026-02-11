namespace ECommercePayment.Domain.DTOs.Response
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Scope { get; set; }
    }
}
