using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommercePayment.Domain.AppSettings;
using ECommercePayment.Domain.Consts;
using ECommercePayment.Domain.DTOs.Request;
using ECommercePayment.Domain.DTOs.Response;
using ECommercePayment.Domain.Entities;
using ECommercePayment.Domain.Enums;
using ECommercePayment.Infrastructure.Cache;
using ECommercePayment.Infrastructure.UOW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ECommercePayment.Application.Services.Identity
{
    public class ECommerceIdentityService : IECommerceIdentityService
    {
        private readonly IUOW _uow;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ECommerceIdentityService> _logger;
        private readonly JwtSettings _jwtSettings;

        public ECommerceIdentityService(
            IUOW uow,
            ICacheService cacheService,
            ILogger<ECommerceIdentityService> logger,
            JwtSettings jwtSettings)
        {
            _uow = uow;
            _cacheService = cacheService;
            _logger = logger;
            _jwtSettings = jwtSettings;
        }

        public async Task<BaseResponse<LoginResponse>> AuthenticateAsync(LoginRequest request)
        {
            var response = new BaseResponse<LoginResponse>();

            try
            {
                // Cache kontrolü
                var cacheKey = $"{CacheKeys.Identity}:{request.Client_ID}";
                var cachedIdentity = await _cacheService.GetAsync<ECommercePaymentIdentity>(cacheKey);

                ECommercePaymentIdentity identity;

                if (cachedIdentity != null)
                {
                    identity = cachedIdentity;
                }
                else
                {
                    identity = await _uow.DbContext.ECommercePaymentIdentity.Where(x =>
                            x.Client_ID == request.Client_ID &&
                            x.Client_Secret == request.Client_Secret &&
                            x.Grant_Type == request.Grant_Type).FirstOrDefaultAsync();

                    if (identity == null)
                    {
                        _logger.LogWarning("Authentication failed for Client_ID: {ClientId}", request.Client_ID);

                        response.ErrorData = new BaseErrorResponse
                        {
                            title = "Authentication Failed",
                            message = "Invalid credentials",
                            errorCode = ErrorCodes.AuthenticationFailed,
                            path = "/api/identity/authenticate",
                            timestamp = DateTime.UtcNow,
                            httpCode = System.Net.HttpStatusCode.Unauthorized,
                            httpMessage = "Unauthorized"
                        };

                        return response;
                    }

                    // Cache'e kaydet
                    await _cacheService.SetAsync(cacheKey, identity, TimeSpan.FromMinutes(30));
                }

                // Token oluştur
                var token = GenerateJwtToken(identity);
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

                _logger.LogInformation("Token generated successfully for Client_ID: {ClientId}", request.Client_ID);

                response.Data = new LoginResponse
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresIn = _jwtSettings.ExpirationMinutes * 60, // saniye cinsinden
                    ExpiresAt = expiresAt,
                    Scope = identity.Scope
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for Client_ID: {ClientId}", request.Client_ID);
                response.ErrorData = new BaseErrorResponse
                {
                    title = "Authentication Exception",
                    message = ex.Message,
                    errorCode = ErrorCodes.AuthenticationFailed,
                    path = "/api/identity/authenticate",
                    timestamp = DateTime.UtcNow,
                    httpCode = System.Net.HttpStatusCode.Unauthorized,
                    httpMessage = "Unauthorized"
                };

                return response;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
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

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }
        private string GenerateJwtToken(ECommercePaymentIdentity identity)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, identity.Id.ToString()),
                new Claim(ClaimTypes.Name, identity.Client_ID),
                new Claim("client_id", identity.Client_ID),
                new Claim("grant_type", identity.Grant_Type),
                new Claim("scope", identity.Scope ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
