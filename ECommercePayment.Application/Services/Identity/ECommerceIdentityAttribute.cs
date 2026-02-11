using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommercePayment.Domain.AppSettings;
using ECommercePayment.Infrastructure.Cache;
using ECommercePayment.Infrastructure.UOW;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ECommercePayment.Application.Services.Identity;

/// <summary>
/// Bearer Authentication attribute for securing controllers
/// Use this attribute on controllers that require authentication
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ECommerceIdentityAttribute : TypeFilterAttribute
{
    public ECommerceIdentityAttribute() : base(typeof(AuthenticationService))
    {
    }
}

public class AuthenticationService : IAsyncAuthorizationFilter
{
    private readonly IUOW _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(
        IUOW uow,
        IHttpContextAccessor httpContextAccessor,
        ICacheService cacheService,
        ILogger<AuthenticationService> logger,
        JwtSettings jwtSettings)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _cacheService = cacheService;
        _logger = logger;
        _jwtSettings = jwtSettings;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            // AllowAnonymous kontrolü
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em is AllowAnonymousAttribute);

            if (allowAnonymous)
            {
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext is null");
                context.Result = new UnauthorizedObjectResult(new { message = "Unauthorized access" });
                return;
            }

            // Authorization header kontrolü
            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Missing or invalid Authorization header");
                context.Result = new UnauthorizedObjectResult(new { message = "Missing or invalid Authorization header" });
                return;
            }

            // Token'ı al
            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token is empty");
                context.Result = new UnauthorizedObjectResult(new { message = "Token is required" });
                return;
            }

            // Token'ı validate et
            var principal = await ValidateTokenAsync(token);
            if (principal == null)
            {
                _logger.LogWarning("Token validation failed");
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid or expired token" });
                return;
            }

            // User'ı context'e ekle
            httpContext.User = principal;
            
            _logger.LogInformation("Token validated successfully for user: {UserId}", 
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication failed" });
        }
    }

    private async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            
            // JWT token olduğunu doğrula
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid token algorithm");
                return null;
            }

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Token has expired");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return null;
        }
    }
}
