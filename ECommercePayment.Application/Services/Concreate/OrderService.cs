using ECommercePayment.Application.Services.Abstaract;
using ECommercePayment.Domain.DTOs;
using ECommercePayment.Domain.DTOs.Request;
using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Domain.Entities;
using ECommercePayment.Domain.Enums;
using ECommercePayment.Infrastructure.UOW;
using ECommercePayment.Integrations.BalanceManagement.Models.Request.Balance;
using ECommercePayment.Integrations.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommercePayment.Application.Services.Concreate;

public class OrderService(
    IBalanceManagementService _balanceManagementService,
    IProductService _productService,
    ILogger<OrderService> _logger,
    IUOW _uow) : IOrderService
{
    public async Task<BaseResponse<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var response = new BaseResponse<CreateOrderResponse>();

        try
        {
            var productsResponse = await _productService.GetProductsAsync(cancellationToken);

            if (productsResponse.Data is null || productsResponse.Data.Count == 0)
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "Product not found",
                    message = "No products available.",
                    errorCode = ErrorCodes.NotFound,
                    httpCode = HttpStatusCode.BadRequest,
                    httpMessage = HttpStatusCode.BadRequest.ToString(),
                    path = "/api/orders/create"
                };

                return response;
            }

            var selectedProducts = productsResponse.Data.Where(x => request.SelectedProducts.Any(p => p.ProductId == x.Id)).ToList();

            if (selectedProducts.Count == 0)
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "Product not found",
                    message = "Requested product does not exist.",
                    errorCode = ErrorCodes.NotFound,
                    httpCode = HttpStatusCode.BadRequest,
                    httpMessage = HttpStatusCode.BadRequest.ToString(),
                    path = "/api/orders/create"
                };

                return response;
            }

            if (selectedProducts.Any(p => p.Stock < request.SelectedProducts.First(x => x.ProductId == p.Id).Quantity))
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "Insufficient stock",
                    message = "Requested quantity exceeds available stock.",
                    errorCode = ErrorCodes.InsufficientStock,
                    httpCode = HttpStatusCode.BadRequest,
                    httpMessage = HttpStatusCode.BadRequest.ToString(),
                    path = "/api/orders/create"
                };

                return response;
            }

            var totalPrice = selectedProducts.Sum(p => p.Price * request.SelectedProducts.First(x => x.ProductId == p.Id).Quantity);

            var balanceResponse = await _balanceManagementService.GetUserBalance();

            if (balanceResponse.Success == true && balanceResponse.Data is not null)
            {
                if (balanceResponse.Data.AvailableBalance < totalPrice)
                {
                    response.ErrorData = new BaseErrorResponse
                    {
                        timestamp = DateTime.UtcNow,
                        title = "Insufficient balance",
                        message = "User does not have enough balance to create preorder.",
                        errorCode = ErrorCodes.InsufficientBalance,
                        httpCode = HttpStatusCode.BadRequest,
                        httpMessage = HttpStatusCode.BadRequest.ToString(),
                        path = "/api/orders/create"
                    };

                    return response;
                }
            }

            Guid orderId = Guid.NewGuid();

            var order = new Orders
            {
                Id = orderId,
                UserId = balanceResponse.Data.UserId,
                TotallPrice = totalPrice,
                Currency = selectedProducts.First().Currency.ToString(),
                Timestamp = DateTime.UtcNow,
                OrderStatus = OrderStatus.Processed,
                Created = DateTime.UtcNow,
                CreatedBy = balanceResponse.Data.UserId,
                Status = DbStatus.ACTIVE,
                OrderedProducts = selectedProducts.Select(p => new OrderedProducts
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    Category = p.Category,
                    Currency = p.Currency,
                    Quantity = request.SelectedProducts.First(x => x.ProductId == p.Id).Quantity,
                    OrderId = orderId
                }).ToList()
            };

            await _uow.DbContext.Orders.AddAsync(order, cancellationToken);

            var preOrderRequest = new PreOrderRequest
            {
                Amount = totalPrice,
                OrderId = orderId.ToString()
            };

            var preOrderResponse = await _balanceManagementService.PreOrder(preOrderRequest);

            if (preOrderResponse.Success == false && preOrderResponse.Data is null)
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "PreOrder service error",
                    message = preOrderResponse.Message,
                    errorCode = preOrderResponse.Error ?? string.Empty,
                    httpCode = HttpStatusCode.BadGateway,
                    httpMessage = HttpStatusCode.BadGateway.ToString(),
                    path = "/api/orders/create"
                };

                return response;
            }

            await _productService.RefreshProductsCacheAsync(cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);

            var preOrder = preOrderResponse.Data.PreOrder;
            var balance = preOrderResponse.Data.UpdatedBalance;

            response.Data = new CreateOrderResponse
            {
                PreOrderId = preOrder.OrderId,
                Status = OrderStatus.Blocked,
                TotalAmount = preOrder.Amount,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Products = order.OrderedProducts.Select(p => new ProductModel
                {
                    Id = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Category = p.Category,
                    Currency = p.Currency,
                    Quantity = p.Quantity
                }).ToList()
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

    public async Task<BaseResponse<CompleteOrderResponse>> CompleteOrderAsync(string preOrderId, CompleteOrderRequest request, CancellationToken cancellationToken = default)
    {
        var response = new BaseResponse<CompleteOrderResponse>();

        try
        {
            var order = await _uow.DbContext.Orders.FirstOrDefaultAsync(o => o.Id == Guid.Parse(preOrderId.ToString()), cancellationToken);

            var balanceResponse = await _balanceManagementService.GetUserBalance();

            if (balanceResponse.Success == true && balanceResponse.Data is not null)
            {
                if (balanceResponse.Data.AvailableBalance < order.TotallPrice)
                {
                    response.ErrorData = new BaseErrorResponse
                    {
                        timestamp = DateTime.UtcNow,
                        title = "Insufficient balance",
                        message = "User does not have enough balance to create preorder.",
                        errorCode = ErrorCodes.InsufficientBalance,
                        httpCode = HttpStatusCode.BadRequest,
                        httpMessage = HttpStatusCode.BadRequest.ToString(),
                        path = "/api/orders/create"
                    };

                    var canceledResponse = await _balanceManagementService.CancelOrder(new CancelRequest { OrderId = preOrderId });

                    if (canceledResponse.Success == false)
                    {
                        _logger.LogError("Failed to cancel order in Balance service for orderId: {OrderId}", preOrderId);
                        response.ErrorData = new BaseErrorResponse
                        {
                            timestamp = DateTime.UtcNow,
                            title = "Cancel order failed",
                            message = "Failed to cancel order in Balance service.",
                            errorCode = ErrorCodes.CancelOrderFailed,
                            httpCode = HttpStatusCode.BadRequest,
                            httpMessage = HttpStatusCode.BadRequest.ToString(),
                            path = "/api/orders/create"
                        };
                    }

                    order.OrderStatus = OrderStatus.Cancelled;
                    order.Modified = DateTime.UtcNow;
                    order.ModifiedBy = balanceResponse.Data.UserId;
                    order.Timestamp = DateTime.UtcNow;

                    _uow.DbContext.Orders.Update(order);

                    await _uow.SaveChangesAsync(cancellationToken);

                    await _productService.RefreshProductsCacheAsync(cancellationToken);

                    return response;
                }
            }

            var productsResponse = await _productService.GetProductsAsync(cancellationToken);

            if (productsResponse.Data is null || productsResponse.Data.Count == 0)
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "Product not found",
                    message = "No products available.",
                    errorCode = ErrorCodes.NotFound,
                    httpCode = HttpStatusCode.BadRequest,
                    httpMessage = HttpStatusCode.BadRequest.ToString(),
                    path = "/api/orders/create"
                };


                var canceledResponse = await _balanceManagementService.CancelOrder(new CancelRequest { OrderId = preOrderId });

                if (canceledResponse.Success == false)
                {
                    _logger.LogError("Failed to cancel order in Balance service for orderId: {OrderId}", preOrderId);
                    response.ErrorData = new BaseErrorResponse
                    {
                        timestamp = DateTime.UtcNow,
                        title = "Cancel order failed",
                        message = "Failed to cancel order in Balance service.",
                        errorCode = ErrorCodes.CancelOrderFailed,
                        httpCode = HttpStatusCode.BadRequest,
                        httpMessage = HttpStatusCode.BadRequest.ToString(),
                        path = "/api/orders/create"
                    };
                }

                order.OrderStatus = OrderStatus.Cancelled;
                order.Modified = DateTime.UtcNow;
                order.ModifiedBy = balanceResponse.Data.UserId;
                order.Timestamp = DateTime.UtcNow;

                _uow.DbContext.Orders.Update(order);

                await _uow.SaveChangesAsync(cancellationToken);

                await _productService.RefreshProductsCacheAsync(cancellationToken);

                return response;
            }

            var selectedProducts = productsResponse.Data.Where(x => order.OrderedProducts.Any(p => p.ProductId == x.Id)).ToList();

            if (selectedProducts.Count == 0)
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "Product not found",
                    message = "Requested product does not exist.",
                    errorCode = ErrorCodes.NotFound,
                    httpCode = HttpStatusCode.BadRequest,
                    httpMessage = HttpStatusCode.BadRequest.ToString(),
                    path = "/api/orders/create"
                };

                var canceledResponse = await _balanceManagementService.CancelOrder(new CancelRequest { OrderId = preOrderId });

                if (canceledResponse.Success == false)
                {
                    _logger.LogError("Failed to cancel order in Balance service for orderId: {OrderId}", preOrderId);
                    response.ErrorData = new BaseErrorResponse
                    {
                        timestamp = DateTime.UtcNow,
                        title = "Cancel order failed",
                        message = "Failed to cancel order in Balance service.",
                        errorCode = ErrorCodes.CancelOrderFailed,
                        httpCode = HttpStatusCode.BadRequest,
                        httpMessage = HttpStatusCode.BadRequest.ToString(),
                        path = "/api/orders/create"
                    };
                }

                order.OrderStatus = OrderStatus.Cancelled;
                order.Modified = DateTime.UtcNow;
                order.ModifiedBy = balanceResponse.Data.UserId;
                order.Timestamp = DateTime.UtcNow;

                _uow.DbContext.Orders.Update(order);

                await _uow.SaveChangesAsync(cancellationToken);

                await _productService.RefreshProductsCacheAsync(cancellationToken);

                return response;
            }

            if (selectedProducts.Any(p => p.Stock < order.OrderedProducts.First(x => x.ProductId == p.Id).Quantity))
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "Insufficient stock",
                    message = "Requested quantity exceeds available stock.",
                    errorCode = ErrorCodes.InsufficientStock,
                    httpCode = HttpStatusCode.BadRequest,
                    httpMessage = HttpStatusCode.BadRequest.ToString(),
                    path = "/api/orders/create"
                };

                var canceledResponse = await _balanceManagementService.CancelOrder(new CancelRequest { OrderId = preOrderId });

                if (canceledResponse.Success == false)
                {
                    _logger.LogError("Failed to cancel order in Balance service for orderId: {OrderId}", preOrderId);
                    response.ErrorData = new BaseErrorResponse
                    {
                        timestamp = DateTime.UtcNow,
                        title = "Cancel order failed",
                        message = "Failed to cancel order in Balance service.",
                        errorCode = ErrorCodes.CancelOrderFailed,
                        httpCode = HttpStatusCode.BadRequest,
                        httpMessage = HttpStatusCode.BadRequest.ToString(),
                        path = "/api/orders/create"
                    };
                }

                order.OrderStatus = OrderStatus.Cancelled;
                order.Modified = DateTime.UtcNow;
                order.ModifiedBy = balanceResponse.Data.UserId;
                order.Timestamp = DateTime.UtcNow;

                _uow.DbContext.Orders.Update(order);

                await _uow.SaveChangesAsync(cancellationToken);

                await _productService.RefreshProductsCacheAsync(cancellationToken);

                return response;
            }

            var completeRequest = new CompleteRequest
            {
                OrderId = preOrderId,
            };

            var completeOrderResponse = await _balanceManagementService.CompleteOrder(completeRequest);

            if (completeOrderResponse.Success == false || completeOrderResponse.Data is null)
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "CompleteOrder service error",
                    message = completeOrderResponse.Message,
                    errorCode = completeOrderResponse.Error ?? string.Empty,
                    httpCode = HttpStatusCode.BadGateway,
                    httpMessage = HttpStatusCode.BadGateway.ToString(),
                    path = "/api/orders/complete"
                };

                var canceledResponse = await _balanceManagementService.CancelOrder(new CancelRequest { OrderId = preOrderId });

                if (canceledResponse.Success == false)
                {
                    _logger.LogError("Failed to cancel order in Balance service for orderId: {OrderId}", preOrderId);
                    response.ErrorData = new BaseErrorResponse
                    {
                        timestamp = DateTime.UtcNow,
                        title = "Cancel order failed",
                        message = "Failed to cancel order in Balance service.",
                        errorCode = ErrorCodes.CancelOrderFailed,
                        httpCode = HttpStatusCode.BadRequest,
                        httpMessage = HttpStatusCode.BadRequest.ToString(),
                        path = "/api/orders/create"
                    };
                }

                order.OrderStatus = OrderStatus.Cancelled;
                order.Modified = DateTime.UtcNow;
                order.ModifiedBy = balanceResponse.Data.UserId;
                order.Timestamp = DateTime.UtcNow;

                _uow.DbContext.Orders.Update(order);

                await _uow.SaveChangesAsync(cancellationToken);

                await _productService.RefreshProductsCacheAsync(cancellationToken);

                return response;
            }

            if (completeOrderResponse.Data.Order.Status.ToString() == OrderStatus.Completed.ToString())
            {
                response.ErrorData = new BaseErrorResponse
                {
                    timestamp = DateTime.UtcNow,
                    title = "CompleteOrder service error",
                    message = "Order is already completed.",
                    errorCode = ErrorCodes.OrderAlreadyCompleted,
                    httpCode = HttpStatusCode.Conflict,
                    httpMessage = HttpStatusCode.Conflict.ToString(),
                    path = "/api/orders/complete"
                };


                var canceledResponse = await _balanceManagementService.CancelOrder(new CancelRequest { OrderId = preOrderId });

                if (canceledResponse.Success == false)
                {
                    _logger.LogError("Failed to cancel order in Balance service for orderId: {OrderId}", preOrderId);
                    response.ErrorData = new BaseErrorResponse
                    {
                        timestamp = DateTime.UtcNow,
                        title = "Cancel order failed",
                        message = "Failed to cancel order in Balance service.",
                        errorCode = ErrorCodes.CancelOrderFailed,
                        httpCode = HttpStatusCode.BadRequest,
                        httpMessage = HttpStatusCode.BadRequest.ToString(),
                        path = "/api/orders/create"
                    };
                }

                order.OrderStatus = OrderStatus.Cancelled;
                order.Modified = DateTime.UtcNow;
                order.ModifiedBy = balanceResponse.Data.UserId;
                order.Timestamp = DateTime.UtcNow;

                _uow.DbContext.Orders.Update(order);

                await _uow.SaveChangesAsync(cancellationToken);

                await _productService.RefreshProductsCacheAsync(cancellationToken);

                return response;
            }

            order.OrderStatus = OrderStatus.Completed;
            order.Modified = DateTime.UtcNow;
            order.ModifiedBy = balanceResponse.Data.UserId;
            order.Timestamp = completeOrderResponse.Data.Order.Timestamp;
            order.CompletedAt = completeOrderResponse.Data.Order.CompletedAt;

            _uow.DbContext.Orders.Update(order);

            await _uow.SaveChangesAsync(cancellationToken);

            response.Data = new CompleteOrderResponse
            {
                OrderId = preOrderId,
                Status = OrderStatus.Completed,
                TotalAmount = 0,
                CompletedAt = DateTime.UtcNow,
                Products = order.OrderedProducts.Select(p => new ProductModel
                {
                    Id = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Category = p.Category,
                    Currency = p.Currency,
                    Quantity = p.Quantity
                }).ToList()
            };

            // cache’i güncelle
            await _productService.RefreshProductsCacheAsync(cancellationToken);

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
