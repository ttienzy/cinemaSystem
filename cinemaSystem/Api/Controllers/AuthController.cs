using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        ITokenClaimService tokenClaimService, 
        IIdentityUserService identityService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await identityService.RegisterUserAsync(request);
            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            return Ok(await identityService.LoginUserAsync(request));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenResponse request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken) || string.IsNullOrEmpty(request.AccessToken))
                return BadRequest("Invalid request");

            var principal = tokenClaimService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal is null)
                return Unauthorized("Invalid access token");

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = principal.FindFirstValue(ClaimTypes.Name);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (userId == null || userName == null || email == null)
                 return Unauthorized("Invalid token claims");

            var isValid = await tokenClaimService.IsRefreshTokenValidAsync(Guid.Parse(userId), request.RefreshToken);
            if (!isValid)
                return Unauthorized("Invalid refresh token");

            var newAccessToken = tokenClaimService.GenerateAccessTokenn(Guid.Parse(userId), userName, email, roles);
            return Ok(new { NewAccessToken = newAccessToken });
        }
    }
}
