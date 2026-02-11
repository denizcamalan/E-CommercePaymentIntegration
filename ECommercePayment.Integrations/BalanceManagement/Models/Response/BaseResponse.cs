namespace ECommercePayment.Integrations.BalanceManagement.Models.Response;

public class BaseResponse<T>
{
    public bool? Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string Message { get; set; } = string.Empty;
}
