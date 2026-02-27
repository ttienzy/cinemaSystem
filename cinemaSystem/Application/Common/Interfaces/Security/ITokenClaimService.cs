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
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken);
    }
}
