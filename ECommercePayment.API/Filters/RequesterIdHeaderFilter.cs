using ECommercePayment.Domain.Consts;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ECommercePayment.API;

public class RequesterIdHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        // X-Requester-Id header'ını ekle
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = HeaderKeys.RequesterId,
            In = ParameterLocation.Header,
            Description = "Unique identifier for the request (for rate limiting and idempotency). Example: req_client123_1707656400_abc123 or UUID",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Example = new Microsoft.OpenApi.Any.OpenApiString("req_client123_1707656400_abc123")
            }
        });
    }
}
