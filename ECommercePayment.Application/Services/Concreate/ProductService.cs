using ECommercePayment.Application.Mappings;
using ECommercePayment.Application.Services.Abstaract;
using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Domain.Enums;
using ECommercePayment.Integrations.BalanceManagement.Models.Response.Products;
using ECommercePayment.Integrations.Services;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommercePayment.Application.Services.Concreate;

public class ProductService : IProductService
{
    private readonly IBalanceManagementService _balanceManagementService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IBalanceManagementService balanceManagementService, ILogger<ProductService> logger)
    {
        _balanceManagementService = balanceManagementService;
        _logger = logger;
    }

    public async Task<BaseResponse<List<ProductResponse>>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var response = new BaseResponse<List<ProductResponse>>
        {
            Data = new List<ProductResponse>()
        };

        try
        {
            Integrations.BalanceManagement.Models.Response.BaseResponse<ProductsResponse> externalResponse = await _balanceManagementService.GetProducts();

            if (externalResponse.Success == true && externalResponse.Data is not null)
            {
                var product = externalResponse.Data.ToProductResponse();
                response.Data = new List<ProductResponse> { product };
                return response;
            }

            response.ErrorData = new BaseErrorResponse
            {
                timestamp = DateTime.UtcNow,
                title = "Balance service error",
                message = externalResponse.Message,
                errorCode = externalResponse.Error ?? string.Empty,
                httpCode = HttpStatusCode.BadGateway,
                httpMessage = HttpStatusCode.BadGateway.ToString(),
                path = "/api/products"
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting products from Balance service.");

            response.ErrorData = new BaseErrorResponse
            {
                timestamp = DateTime.UtcNow,
                title = "Internal error",
                message = ex.Message,
                errorCode = ErrorCodes.InternalError,
                httpCode = HttpStatusCode.InternalServerError,
                httpMessage = HttpStatusCode.InternalServerError.ToString(),
                path = "/api/products"
            };

            return response;
        }
    }
}
