using ECommercePayment.Domain.DTOs.Request;
using ECommercePayment.Domain.DTOs.Response;

namespace ECommercePayment.Application.Services.Identity
{
    public interface IECommerceIdentityService
    {
        Task<BaseResponse<LoginResponse>> AuthenticateAsync(LoginRequest request);
        Task<bool> ValidateTokenAsync(string token);
    }
}
