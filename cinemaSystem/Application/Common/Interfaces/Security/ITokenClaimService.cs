using Shared.Models.IdentityModels;
using System.Security.Claims;

namespace Application.Common.Interfaces.Security
{
    /// <summary>
    /// JWT token generation and validation.
    /// </summary>
    public interface ITokenClaimService
    {
        string GenerateAccessTokenn(Guid userId, string userName, string email, List<string> roles);
        string GenerateRefreshToken();
        DateTime GetRefreshTokenExpirationTime();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken);
        /// <summary>
        /// Revoke refresh token — used for logout functionality.
        /// </summary>
        Task RevokeRefreshTokenAsync(Guid userId);
        /// <summary>
        /// Store refresh token in cache.
        /// </summary>
        Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiration);
    }
}
