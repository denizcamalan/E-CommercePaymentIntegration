using ECommercePayment.Application.Services.Abstaract;
using ECommercePayment.Domain.DTOs.Request;
using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Domain.Enums;
using ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;
using ECommercePayment.Integrations.Services;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommercePayment.Application.Services.Concreate;

public class OrderService(IBalanceManagementService _balanceManagementService, ILogger<OrderService> _logger) : IOrderService
{
    public async Task<BaseResponse<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var response = new BaseResponse<CreateOrderResponse>();

        try
        {
            if (request.TotalPrice is null || request.TotalPrice <= 0)
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "Validation error",
                    message = "TotalAmount must be greater than zero.",
                    errorCode = ErrorCodes.InvalidFormat,
                    httpCode = HttpStatusCode.BadRequest,
                    httpMessage = HttpStatusCode.BadRequest.ToString(),
                    path = "/api/orders/create"
                };

                return response;
            }

            var preOrderRequest = new PreOrderRequest
            {
                Amount = request.TotalPrice.Value,
                OrderId = Guid.NewGuid().ToString()
            };

            var externalResponse = await _balanceManagementService.PreOrder(preOrderRequest);

            if (externalResponse.Success == true && externalResponse.Data is not null)
            {
                var preOrder = externalResponse.Data.PreOrder;
                var balance = externalResponse.Data.UpdatedBalance;

                response.Data = new CreateOrderResponse
                {
                    CartId = Guid.NewGuid().ToString(), // Şimdilik in-memory temsil; ileride Cart entity ile ilişkilendirilebilir.
                    PreOrderId = preOrder.OrderId,
                    Status = preOrder.Status.ToString(),
                    TotalAmount = preOrder.Amount,
                    BlockedAmount = balance?.BlockedBalance ?? preOrder.Amount,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };

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
                path = "/api/orders/create"
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating preorder with Balance service.");

            response.ErrorData = new BaseErrorResponse
            {
                timestamp = DateTime.UtcNow,
                title = "Internal error",
                message = ex.Message,
                errorCode = ErrorCodes.InternalError,
                httpCode = HttpStatusCode.InternalServerError,
                httpMessage = HttpStatusCode.InternalServerError.ToString(),
                path = "/api/orders/create"
            };

            return response;
        }
    }

    public async Task<BaseResponse<CompleteOrderResponse>> CompleteOrderAsync(CompleteOrderRequest request, CancellationToken cancellationToken = default)
    {
        var response = new BaseResponse<CompleteOrderResponse>();

        try
        {
            var completeRequest = new CompleteRequest
            {
                OrderId = request.PreOrderId
            };

            var externalResponse = await _balanceManagementService.CompleteOrder(completeRequest);

            if (externalResponse.Success == true)
            {
                response.Data = new CompleteOrderResponse
                {
                    OrderId = request.PreOrderId,
                    Status = OrderStatus.Completed,
                    TotalAmount = 0, // Şu an external response detay içermiyor; ileride genişletilebilir.
                    CompletedAt = DateTime.UtcNow
                };

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
                path = "/api/orders/complete"
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while completing order with Balance service.");

            response.ErrorData = new BaseErrorResponse
            {
                timestamp = DateTime.UtcNow,
                title = "Internal error",
                message = ex.Message,
                errorCode = ErrorCodes.InternalError,
                httpCode = HttpStatusCode.InternalServerError,
                httpMessage = HttpStatusCode.InternalServerError.ToString(),
                path = "/api/orders/complete"
            };

            return response;
        }
    }
}
