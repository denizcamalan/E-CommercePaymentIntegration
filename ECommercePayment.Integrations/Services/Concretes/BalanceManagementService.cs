using ECommercePayment.Domain.AppSettings;
using ECommercePayment.Integrations.BalanceManagement.Consts;
using ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Products;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace ECommercePayment.Integrations.Services;

public class BalanceManagementService(ILogger<BalanceManagementService> _logger, IHttpClientFactory _httpClientFactory, BalanceManagementSettings _bMSettings) : IBalanceManagementService
{
    public async Task<BaseResponse<CancelResponse>> CancelOrder(CancelRequest request)
    {
        _logger.LogInformation($"CancelOrder Request : {JsonConvert.SerializeObject(request, Formatting.None)}");

        BaseResponse<CancelResponse> response = await GetResponse<CancelResponse, CancelRequest>("/api/balance/cancel", HttpMethod.Post, request);

        _logger.LogInformation($"CancelOrder Response : {JsonConvert.SerializeObject(response, Formatting.None)}");

        return response;
    }

    public async Task<BaseResponse<CompleteResponse>> CompleteOrder(CompleteRequest request)
    {
        _logger.LogInformation($"CompleteOrder Request : {JsonConvert.SerializeObject(request, Formatting.None)}");

        BaseResponse<CompleteResponse> response = await GetResponse<CompleteResponse, CompleteRequest>("/api/balance/complete", HttpMethod.Post, request);

        _logger.LogInformation($"CompleteOrder Response : {JsonConvert.SerializeObject(response, Formatting.None)}");

        return response;
    }

    public async Task<BaseResponse<ProductsResponse>> GetProducts()
    {
        _logger.LogInformation($"GetProducts");

        BaseResponse<ProductsResponse> response = await GetResponse<ProductsResponse>("/api/products", HttpMethod.Post);

        _logger.LogInformation($"GetProducts Response : {JsonConvert.SerializeObject(response, Formatting.None)}");

        return response;
    }

    public async Task<BaseResponse<UserBalanceResponse>> GetUserBalance()
    {
        _logger.LogInformation($"GetUserBalance userId ");

        BaseResponse<UserBalanceResponse> response = await GetResponse<UserBalanceResponse>("/api/balance", HttpMethod.Post);

        _logger.LogInformation($"GetUserBalance Response : {JsonConvert.SerializeObject(response, Formatting.None)}");

        return response;
    }

    public async Task<BaseResponse<PreOrderResponse>> PreOrder(PreOrderRequest request)
    {
        _logger.LogInformation($"PreOrder Request : {JsonConvert.SerializeObject(request, Formatting.None)}");

        BaseResponse<PreOrderResponse> response = await GetResponse<PreOrderResponse, PreOrderRequest>("/api/balance/preorder", HttpMethod.Post, request);

        _logger.LogInformation($"PreOrder Response : {JsonConvert.SerializeObject(response, Formatting.None)}");

        return response;
    }

    #region PrivateMethods
    private async Task<BaseResponse<Tout>> GetResponse<Tout, Tin>(string address, HttpMethod method, Tin model, string contentType = "application/json", bool isResponseConvert = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _bMSettings.BaseUrl;

            var payload = JsonConvert.SerializeObject(model);
            var httpContent = new StringContent(payload, Encoding.UTF8, contentType);

            Uri url = new Uri($"{baseUrl}{address}");

            HttpRequestMessage request = new HttpRequestMessage(method, url)
            {
                Content = httpContent
            };

            HttpClient httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage result = await httpClient.SendAsync(request, cancellationToken);

            var body = await result.Content.ReadAsStringAsync();

            var response = new BaseResponse<Tout>();

            if (!string.IsNullOrEmpty(body))
            {
                var deserilizeObj = JsonConvert.DeserializeObject<BaseResponse<Tout>>(body);

                if (deserilizeObj is not null)
                {
                    response = deserilizeObj;
                }
            }

            if (!result.IsSuccessStatusCode && string.IsNullOrEmpty(response.Error))
            {
                response.Error = result.StatusCode.ToString();
                response.Message = ErrorMesssages.AccessError;
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(GetResponse)} : {ex.Message}");

            return new BaseResponse<Tout>
            {
                Error = ErrorMesssages.InternalService,
                Message = ex.Message
            };
        }

    }

    private async Task<BaseResponse<Tout>> GetResponse<Tout>(string address, HttpMethod method,string contentType = "application/json", bool isResponseConvert = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _bMSettings.BaseUrl;

            Uri url = new Uri($"{baseUrl}{address}");

            HttpRequestMessage request = new HttpRequestMessage(method, url);

            HttpClient httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage result = await httpClient.SendAsync(request, cancellationToken);

            var body = await result.Content.ReadAsStringAsync();

            var response = new BaseResponse<Tout>();

            if (!string.IsNullOrEmpty(body))
            {
                var deserilizeObj = JsonConvert.DeserializeObject<BaseResponse<Tout>>(body);

                if (deserilizeObj is not null)
                {
                    response = deserilizeObj;
                }
            }

            if (!result.IsSuccessStatusCode && string.IsNullOrEmpty(response.Error))
            {
                response.Error = result.StatusCode.ToString();
                response.Message = ErrorMesssages.AccessError;
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(GetResponse)} : {ex.Message}");

            return new BaseResponse<Tout>
            {
                Error = ErrorMesssages.InternalService,
                Message = ex.Message
            };
        }

    }
    #endregion
}
