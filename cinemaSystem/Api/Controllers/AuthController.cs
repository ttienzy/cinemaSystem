using Application.Common.Interfaces.Security;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Models.IdentityModels;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenClaimService _tokenClaimService;
        private readonly IIdentityUserService _identityService;
        public AuthController(ITokenClaimService tokenClaimService, IIdentityUserService identityUserService)
        {
            _tokenClaimService = tokenClaimService ?? throw new ArgumentNullException(nameof(tokenClaimService), "TokenClaimService cannot be null");
            _identityService = identityUserService ?? throw new ArgumentNullException(nameof(identityUserService), "IdentityUserService cannot be null");
        }

        // User Registration, Login, Logout Operations
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest registerRequest)
        {
            if (registerRequest == null)
            {
                return BadRequest("Register request cannot be null.");
            }
            var response = await _identityService.RegisterUserAsync(registerRequest);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<string>.WithError(response);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null)
            {
                return BadRequest("Login request cannot be null.");
            }
            var response = await _identityService.LoginUserAsync(loginRequest);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<LoginResponse>.WithError(response);
        }




        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] TokenResponse request)
        {
            if (request == null || string.IsNullOrEmpty(request.RefreshToken) || string.IsNullOrEmpty(request.AccessToken))
            {
                return BadRequest("Invalid request");
            }
            try
            {
                var principal = _tokenClaimService.GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal is null)
                {
                    return Unauthorized("Invalid access token");
                }
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = principal.FindFirstValue(ClaimTypes.Name);
                var email = principal.FindFirstValue(ClaimTypes.Email);
                var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var IsValidRefreshToken = await _tokenClaimService.IsRefreshTokenValidAsync(Guid.Parse(userId), request.RefreshToken);
                if (IsValidRefreshToken)
                {
                    var newAccessToken = _tokenClaimService.GenerateAccessTokenn(Guid.Parse(userId), userName, email, roles);
                    return Ok(new { NewAccessToken = newAccessToken });
                }
                else
                {
                    return Unauthorized("Invalid refresh token");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

    }
}
