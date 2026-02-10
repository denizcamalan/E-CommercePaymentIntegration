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
}
