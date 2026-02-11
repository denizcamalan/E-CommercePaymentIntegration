using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ECommercePayment.Domain.Consts;
using ECommercePayment.Domain.Models.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommercePayment.Application.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var originalResponseBody = context.Response.Body;
            using var newResponseBody = new MemoryStream();
            context.Response.Body = newResponseBody;

            try
            {
                // Endpoint'i çağır
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred. CorrelationId: {CorrelationId}");
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Response loglama
                await LogResponseAsync(context, stopwatch.ElapsedMilliseconds);

                // Response body'yi geri kopyala
                newResponseBody.Seek(0, SeekOrigin.Begin);
                await newResponseBody.CopyToAsync(originalResponseBody);
            }
        }

        private async Task<HttpLogRequestModel> LogRequestAsync(HttpContext context, string correlationId)
        {
            var request = context.Request;

            // Request body'yi oku
            request.EnableBuffering();
            string? requestBody = null;
            
            if (request.ContentLength > 0)
            {
                try
                {
                    request.Body.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync();
                    request.Body.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read request body. CorrelationId: {CorrelationId}", correlationId);
                }
            }

            var requestLog = new HttpLogRequestModel
            {
                CorrelationId = correlationId,
                Scheme = request.Scheme,
                Host = request.Host.ToString(),
                Path = request.Path.Value ?? string.Empty,
                Method = request.Method,
                QueryString = request.QueryString.Value,
                ContentType = request.ContentType ?? "-",
                IPAddress = context.Connection.RemoteIpAddress?.ToString() ?? "-",
                Timestamp = DateTime.UtcNow
            };

            // Headers (sensitive header'ları filtrele)
            requestLog.Headers = request.Headers
                .Where(h => !IsSensitiveHeader(h.Key))
                .Select(h => new KeyValuePair<string, object>(h.Key, h.Value.ToString()))
                .ToDictionary(k => k.Key, v => v.Value);

            // Request body'yi parse et
            if (!string.IsNullOrEmpty(requestBody))
            {
                try
                {
                    requestLog.Body = JsonSerializer.Deserialize<object>(requestBody);
                    context.Items["RequestBody"] = requestBody;
                }
                catch
                {
                    // JSON değilse raw text olarak kaydet
                    requestLog.Body = requestBody.Length > 1000 ? requestBody.Substring(0, 1000) + "..." : requestBody;
                }
            }

            _logger.LogInformation("HTTP Request: {Request}", JsonSerializer.Serialize(requestLog, new JsonSerializerOptions 
            { 
                WriteIndented = false 
            }));

            return requestLog;
        }

        private async Task LogResponseAsync(HttpContext context, long elapsedMilliseconds)
        {
            var response = context.Response;

            // Response body'yi oku
            string? responseBody = null;
            if (response.Body.CanSeek)
            {
                try
                {
                    response.Body.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
                    responseBody = await reader.ReadToEndAsync();
                    response.Body.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read response body. CorrelationId: {CorrelationId}");
                }
            }

            var responseLog = new HttpLogResponseModel
            {
                StatusCode = response.StatusCode,
                ContentType = response.ContentType,
                Timestamp = DateTime.UtcNow,
                ElapsedMilliseconds = elapsedMilliseconds
            };

            // Headers (sensitive header'ları filtrele)
            responseLog.Headers = response.Headers
                .Where(h => !IsSensitiveHeader(h.Key))
                .Select(h => new KeyValuePair<string, object>(h.Key, h.Value.ToString()))
                .ToDictionary(k => k.Key, v => v.Value);

            // Response body'yi parse et
            if (!string.IsNullOrEmpty(responseBody))
            {
                try
                {
                    responseLog.Body = JsonSerializer.Deserialize<object>(responseBody);
                }
                catch
                {
                    // JSON değilse raw text olarak kaydet
                    responseLog.Body = responseBody.Length > 1000 ? responseBody.Substring(0, 1000) + "..." : responseBody;
                }
            }

            var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
                          response.StatusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(logLevel, "HTTP Response: {Response}", JsonSerializer.Serialize(responseLog, new JsonSerializerOptions 
            { 
                WriteIndented = false 
            }));
        }

        private bool IsSensitiveHeader(string headerName)
        {
            var sensitiveHeaders = new[]
            {
                "Authorization",
                "Cookie",
                "Set-Cookie",
                "X-Api-Key",
                "X-Auth-Token",
                "Password",
                "Client-Secret"
            };

            return sensitiveHeaders.Any(h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
