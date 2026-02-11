using System.Net;
using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Domain.Enums;
using ECommercePayment.Domain.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ECommercePayment.Application.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> _logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (TaskCanceledException ex)
        {
            if (httpContext.RequestAborted.IsCancellationRequested)
            {

                BaseErrorResponse response = new BaseErrorResponse()
                {
                    httpCode = HttpStatusCode.GatewayTimeout,
                    path = httpContext.Request.Path,
                    errorCode = ErrorCodes.RequestTimeout,
                    timestamp = DateTime.UtcNow,
                    httpMessage = HttpStatusCode.GatewayTimeout.ToString(),
                    message = "Request timed out. The server took too long to respond."
                };

                _logger.LogError($"Timeout : {ex.Message}");

                await HttpExtensions.ResponseOverride(httpContext, response.httpCode, response);

                return;
            }
        }
        catch (Exception ex)
        {

            BaseErrorResponse response = new BaseErrorResponse()
            {
                httpCode = HttpStatusCode.InternalServerError,
                path = httpContext.Request.Path,
                errorCode = ErrorCodes.InternalServerError,
                timestamp = DateTime.UtcNow,
                httpMessage = HttpStatusCode.InternalServerError.ToString(),
                message = "An internal server error occurred."
            };

            _logger.LogError(ex.Message);

            await HttpExtensions.ResponseOverride(httpContext, response.httpCode, response);
        }
        return;
    }
}

