using System.Net;
using ECommercePayment.Domain.DTOs.Response;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ECommercePayment.Domain.Extentions;

public static class HttpExtensions
{
     public static async Task ResponseOverride(this HttpContext context, HttpStatusCode statusCode, BaseErrorResponse response, string? contentType = "application/json")
    {
        if (!string.IsNullOrWhiteSpace(contentType))
            context.Response.ContentType = contentType;
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        }));
        return;
    }
}
