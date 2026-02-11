namespace ECommercePayment.Domain.Enums;

public static class ErrorCodes
{
    public const string InvalidFormat = "InvalidFormat";
    public const string NotFound = "NotFound";
    public const string UnAuthorize = "UnAuthorize";
    public const string InternalError = "InternalError";
    public const string InsufficientBalance = "InsufficientBalance";
    public const string OrderAlreadyProcessed = "OrderAlreadyProcessed";
    public const string OrderNotFound = "OrderNotFound";
    public const string InsufficientStock = "InsufficientStock";
    public const string OrderAlreadyCompleted = "OrderAlreadyCompleted";
    public const string CancelOrderFailed = "CancelOrderFailed";
    public const string AuthenticationFailed = "AuthenticationFailed";
    public const string InvalidToken = "InvalidToken";
    public const string TokenExpired = "TokenExpired";
    public const string InternalServerError = "InternalServerError";
    public const string ValidationFailed = "ValidationFailed";
    public const string RateLimitExceeded = "RateLimitExceeded";
    public const string RequesterIdMissing = "RequesterIdMissing";
    public const string RequestTimeout = "RequestTimeout";
}
