using System.Text;
using ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Products;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ECommercePayment.Integrations.Services;

public class BalanceManagementService(ILogger<BalanceManagementService> _logger, IHttpClientFactory _httpClientFactory) : IBalanceManagementService
{
    public CancelResponse CancelOrder(CancelRequest request)
    {
        throw new NotImplementedException();
    }

    public CompleteResponse CompleteOrder(CompleteRequest request)
    {
        throw new NotImplementedException();
    }

    public ProductsResponse GetProducts()
    {
        throw new NotImplementedException();
    }

    public UserBalanceResponse GetUserBalance(string userId)
    {
        throw new NotImplementedException();
    }

    public PreOrderResponse PreOrder(PreOrderRequest request)
    {
        throw new NotImplementedException();
    }

    // private async Task<BaseResponse<Tout>> GetResponse<Tout, Tin>(string address, HttpMethod method, Tin model, string contentType = "application/json", bool isResponseConvert = true, CancellationToken cancellationToken = default)
    // {
    //     try
    //     {
    //         var payload = JsonConvert.SerializeObject(model);
    //         var httpContent = new StringContent(payload, Encoding.UTF8, contentType);

    //         Uri url = new Uri($"{"url"}{address}");

    //         HttpRequestMessage request = new HttpRequestMessage(method, url)
    //         {
    //             Content = httpContent
    //         };


    //         HttpClient httpClient = _httpClientFactory.CreateClient();

    //         HttpResponseMessage result = await httpClient.SendAsync(request, cancellationToken);

    //         var body = await result.Content.ReadAsStringAsync();

    //         var response = new BaseResponse<Tout>
    //         {
    //             Success = result.IsSuccessStatusCode,
    //         };

    //         if (!string.IsNullOrEmpty(body))
    //         {
    //             if (result.IsSuccessStatusCode)
    //             {
    //                 if (isResponseConvert)
    //                     response.Data = JsonConvert.DeserializeObject<Tout>(body);
    //             }
    //             else
    //             {
    //                 response.Error = result.
    //             }
    //         }
    //         else if (!response.isSuccess && response.Error is null)
    //         {
    //             response.Error = new()
    //             {
    //                 HataMesajiEN = "Access error! Please, try again.",
    //                 HataMesaji = "Erişim hatası! Tekrar deneyeniz.",
    //                 Kod = Enums.WalletResponseCodes.HATA
    //             };
    //         }

    //         if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    //         {
    //             await _cacheService.Delete(CacheConsts.WALLET_TOKEN);
    //         }

    //         return response;
    //     }
    //     catch (Exception ex)
    //     {

    //         _logger.LogError($"{nameof(GetResponse)} : {ex.Message}");

    //         return new WBaseResponse<Tout>
    //         {
    //             isSuccess = false,
    //             StatusCode = System.Net.HttpStatusCode.BadRequest,
    //             Error = new()
    //             {
    //                 HataMesajiEN = "System error! Please, try again.",
    //                 HataMesaji = "Sistem hatası! Tekrar deneyeniz.",
    //                 Kod = Enums.WalletResponseCodes.BILINMEYEN
    //             }
    //         };
    //     }

    // }
}
