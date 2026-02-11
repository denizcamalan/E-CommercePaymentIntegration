using System.Text;
using ECommercePayment.Domain.AppSettings;
using ECommercePayment.Integrations.BalanceManagement.Consts;
using ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Balance;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Products;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace ECommercePayment.Integrations.Services;

public class BalanceManagementService(ILogger<BalanceManagementService> _logger, IHttpClientFactory _httpClientFactory, BalanceManagementSettings _bMSettings) : IBalanceManagementService
{
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy =
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => (int)msg.StatusCode >= 500)
            .WaitAndRetryAsync(
                4,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retrying BalanceManagement request. Attempt {RetryCount}", retryCount);
                });
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

    public async Task<BaseResponse<List<ProductsResponse>>> GetProducts()
    {
        _logger.LogInformation($"GetProducts");

        BaseResponse<List<ProductsResponse>> response = await GetResponse<List<ProductsResponse>>("/api/products", HttpMethod.Get);

        _logger.LogInformation($"GetProducts Response : {JsonConvert.SerializeObject(response, Formatting.None)}");

        return response;
    }

    public async Task<BaseResponse<UserBalanceResponse>> GetUserBalance()
    {
        _logger.LogInformation($"GetUserBalance userId ");

        BaseResponse<UserBalanceResponse> response = await GetResponse<UserBalanceResponse>("/api/balance", HttpMethod.Get);

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

            HttpResponseMessage result = await _retryPolicy.ExecuteAsync(ct =>
            httpClient.PostAsync(url, new StringContent(payload, Encoding.UTF8, contentType), ct), cancellationToken);

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
                response.Message = (int)result.StatusCode >= 500
                    ? "Lütfen daha sonra tekrar deneyin."
                    : ErrorMesssages.AccessError;
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(GetResponse)} : {ex.Message}");

            return new BaseResponse<Tout>
            {
                Error = ErrorMesssages.InternalService,
                Message = "Lütfen daha sonra tekrar deneyin."
            };
        }

    }

    private async Task<BaseResponse<Tout>> GetResponse<Tout>(string address, HttpMethod method, string contentType = "application/json", bool isResponseConvert = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _bMSettings.BaseUrl;

            Uri url = new Uri($"{baseUrl}{address}");

            HttpRequestMessage request = new HttpRequestMessage(method, url);

            HttpClient httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage result = await _retryPolicy
                .ExecuteAsync(ct => httpClient.SendAsync(request, ct), cancellationToken);

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
                response.Message = (int)result.StatusCode >= 500
                    ? "Lütfen daha sonra tekrar deneyin."
                    : ErrorMesssages.AccessError;
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(GetResponse)} : {ex.Message}");

            return new BaseResponse<Tout>
            {
                Error = ErrorMesssages.InternalService,
                Message = "Lütfen daha sonra tekrar deneyin."
            };
        }

    }
    #endregion
}
