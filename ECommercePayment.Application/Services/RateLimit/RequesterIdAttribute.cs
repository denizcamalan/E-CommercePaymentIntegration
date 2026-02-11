using ECommercePayment.Domain.Consts;
using ECommercePayment.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ECommercePayment.Application.Services.RateLimit
{
    /// <summary>
    /// RequesterID bazlı rate limiting ve idempotency attribute
    /// Header'dan X-Requester-Id okur, rate limiting ve cache kontrolü yapar
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequesterIdAttribute : TypeFilterAttribute
    {
        public RequesterIdAttribute() : base(typeof(RequesterIdFilter))
        {
        }
    }

    public class RequesterIdFilter : IAsyncActionFilter
    {
        private readonly IRateLimitService _rateLimitService;
        private readonly ILogger<RequesterIdFilter> _logger;

        public RequesterIdFilter(
            IRateLimitService rateLimitService,
            ILogger<RequesterIdFilter> logger)
        {
            _rateLimitService = rateLimitService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                if (!context.HttpContext.Request.Headers.TryGetValue(HeaderKeys.RequesterId, out var requesterIdHeader) ||
                    string.IsNullOrWhiteSpace(requesterIdHeader))
                {
                    _logger.LogWarning("Missing or empty X-Requester-Id header");
                    context.Result = new BadRequestObjectResult(new
                    {
                        success = false,
                        errorCode = ErrorCodes.InvalidFormat,
                        message = "X-Requester-Id header is required"
                    });
                    return;
                }

                var requesterId = requesterIdHeader.ToString();
                var endpoint = context.HttpContext.Request.Path.Value ?? string.Empty;
                var method = context.HttpContext.Request.Method;

                _logger.LogInformation("Processing request with RequesterID: {RequesterId}, Endpoint: {Endpoint}, Method: {Method}",
                    requesterId, endpoint, method);

                var isRateLimitExceeded = await _rateLimitService.IsRateLimitExceededAsync(requesterId);
                if (isRateLimitExceeded)
                {
                    _logger.LogWarning("Rate limit exceeded for RequesterID: {RequesterId}", requesterId);
                    context.Result = new ObjectResult(new
                    {
                        success = false,
                        errorCode = ErrorCodes.RateLimitExceeded,
                        message = "Rate limit exceeded. Please try again later."
                    })
                    {
                        StatusCode = 429 // Too Many Requests
                    };
                    return;
                }
                if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                    method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                    method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
                {
                    var cachedResponse = await _rateLimitService.GetCachedResponseAsync<object>(requesterId, endpoint);
                    if (cachedResponse != null)
                    {
                        _logger.LogInformation("Returning cached response for RequesterID: {RequesterId}, Endpoint: {Endpoint}",
                            requesterId, endpoint);
                        
                        context.Result = new OkObjectResult(cachedResponse);
                        return;
                    }
                }

                await _rateLimitService.IncrementRequestCountAsync(requesterId);

                var executedContext = await next();

                if (executedContext.Result is ObjectResult objectResult && 
                    objectResult.StatusCode is >= 200 and < 300)
                {
                    if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                        method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                        method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
                    {
                        await _rateLimitService.CacheResponseAsync(requesterId, endpoint, objectResult.Value);
                        _logger.LogInformation("Cached response for RequesterID: {RequesterId}, Endpoint: {Endpoint}",
                            requesterId, endpoint);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RequesterIdFilter");
                context.Result = new ObjectResult(new
                {
                    success = false,
                    errorCode = ErrorCodes.InternalServerError,
                    message = "An error occurred while processing the request"
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}
