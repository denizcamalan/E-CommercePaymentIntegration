using ECommercePayment.Application.Mappings;
using ECommercePayment.Application.Services.Abstaract;
using ECommercePayment.Domain.Consts;
using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Domain.Enums;
using ECommercePayment.Infrastructure.Cache;
using ECommercePayment.Integrations.Services;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommercePayment.Application.Services.Concreate;

public class ProductService : IProductService
{
    private readonly IBalanceManagementService _balanceManagementService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IBalanceManagementService balanceManagementService,
        ICacheService cacheService,
        ILogger<ProductService> logger)
    {
        _balanceManagementService = balanceManagementService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<BaseResponse<List<ProductResponse>>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var response = new BaseResponse<List<ProductResponse>> { Data = new List<ProductResponse>() };

        var cached = await _cacheService.GetAsync<List<ProductResponse>>(CacheKeys.ProductsCacheKey);
        if (cached is not null && cached.Count > 0)
        {
            response.Data = cached;
            return response;
        }

        return await RefreshProductsInternalAsync(response, allowStaleOnError: false, cancellationToken);
    }

    public async Task RefreshProductsCacheAsync(CancellationToken cancellationToken = default)
    {
        await RefreshProductsInternalAsync(new BaseResponse<List<ProductResponse>>(), allowStaleOnError: true, cancellationToken);
    }

    private async Task<BaseResponse<List<ProductResponse>>> RefreshProductsInternalAsync(
        BaseResponse<List<ProductResponse>> response,
        bool allowStaleOnError,
        CancellationToken cancellationToken)
    {
        try
        {
            var externalResponse = await _balanceManagementService.GetProducts();

            if (externalResponse.Success == true && externalResponse.Data is not null)
            {
                var product = externalResponse.Data.ToProductResponse();
                var list = new List<ProductResponse> { product };

                await _cacheService.SetAsync(CacheKeys.ProductsCacheKey, list, TimeSpan.FromMinutes(5));

                response.Data = list;
                return response;
            }

            response.ErrorData ??= new BaseErrorResponse
            {
                timestamp = DateTime.UtcNow,
                title = "Balance service error",
                message = externalResponse.Message,
                errorCode = externalResponse.Error ?? string.Empty,
                httpCode = HttpStatusCode.InternalServerError,
                httpMessage = HttpStatusCode.InternalServerError.ToString(),
                path = "/api/products"
            };

            if (allowStaleOnError)
            {
                var stale = await _cacheService.GetAsync<List<ProductResponse>>(CacheKeys.ProductsCacheKey);
                if (stale is not null && stale.Count > 0)
                {
                    _logger.LogWarning("Balance service unavailable. Serving products from stale Redis cache.");
                    response.Data = stale;
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting products from Balance service.");

            response.ErrorData ??= new BaseErrorResponse
            {
                timestamp = DateTime.UtcNow,
                title = "Internal error",
                message = ex.Message,
                errorCode = ErrorCodes.InternalError,
                httpCode = HttpStatusCode.InternalServerError,
                httpMessage = HttpStatusCode.InternalServerError.ToString(),
                path = "/api/products"
            };

            if (allowStaleOnError)
            {
                var stale = await _cacheService.GetAsync<List<ProductResponse>>(CacheKeys.ProductsCacheKey);
                if (stale is not null && stale.Count > 0)
                {
                    _logger.LogWarning("Balance service unavailable. Serving products from stale Redis cache.");
                    response.Data = stale;
                }
            }

            return response;
        }
    }
}
