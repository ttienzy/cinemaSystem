using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Xác thực người dùng — đăng ký, đăng nhập, refresh token, đăng xuất.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        ITokenClaimService tokenClaimService,
        IIdentityUserService identityService) : ControllerBase
    {
        /// <summary>
        /// Đăng ký tài khoản mới.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await identityService.RegisterUserAsync(request);
            return Ok("User registered successfully.");
        }

        /// <summary>
        /// Đăng nhập — trả về access token + refresh token.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            return Ok(await identityService.LoginUserAsync(request));
        }

        /// <summary>
        /// Refresh access token — dùng refresh token để lấy access token mới.
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
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
            var newRefreshToken = tokenClaimService.GenerateRefreshToken();
            var refreshTokenExpiry = tokenClaimService.GetRefreshTokenExpirationTime();

            // Revoke old refresh token and store new one
            await tokenClaimService.RevokeRefreshTokenAsync(Guid.Parse(userId));
            await tokenClaimService.StoreRefreshTokenAsync(Guid.Parse(userId), newRefreshToken, refreshTokenExpiry);

            return Ok(new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        /// <summary>
        /// Đăng xuất — hủy refresh token, buộc user phải đăng nhập lại.
        /// </summary>
        [HttpPost("logout")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                await tokenClaimService.RevokeRefreshTokenAsync(Guid.Parse(userId));
            }
            return Ok(new { message = "Đăng xuất thành công." });
        }
    }
}
