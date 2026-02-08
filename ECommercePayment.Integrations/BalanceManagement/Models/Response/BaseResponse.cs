namespace ECommercePayment.Integrations.Models;

public class BaseResponse<T> where T : class
{
    public bool? Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string Message { get; set; } = string.Empty;
}
