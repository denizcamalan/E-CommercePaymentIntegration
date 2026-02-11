using ECommercePayment.Application.Services.Identity;
using ECommercePayment.Domain.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePayment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController : BaseController
    {
        private readonly IECommerceIdentityService _identityService;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(
            IECommerceIdentityService identityService,
            ILogger<IdentityController> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate and get access token
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Access token with expiration details</returns>
        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetToken([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Token request received for Client_ID: {ClientId}", request.Client_ID);

            var response = await _identityService.AuthenticateAsync(request);

            if (response.ErrorData is not null)
            {
                _logger.LogWarning("Token generation failed for Client_ID: {ClientId}", request.Client_ID);
                return Unauthorized(response);
            }

            _logger.LogInformation("Token generated successfully for Client_ID: {ClientId}", request.Client_ID);
            return Ok(response);
        }

        /// <summary>
        /// Validate current token (for testing purposes)
        /// </summary>
        /// <returns>Token validation status</returns>
        [HttpGet("validate")]
        [ECommerceIdentity]
        public IActionResult ValidateToken()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var clientId = User.FindFirst("client_id")?.Value;

            _logger.LogInformation("Token validated for User ID: {UserId}, Client ID: {ClientId}", userId, clientId);

            return Ok(new
            {
                success = true,
                message = "Token is valid",
                userId = userId,
                clientId = clientId,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
    }
}
