namespace ECommercePayment.Domain.DTOs.Response;

public class BaseResponse<T> where T : class
{
    public T? Data { get; set; }
    public BaseErrorResponse? ErrorData { get; set; }
}
