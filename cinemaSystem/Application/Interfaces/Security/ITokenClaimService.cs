using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Security
{
    public interface ITokenClaimService
    {
        public string GenerateAccessTokenn(Guid userId, string userName, string email, List<string> roles);
        public string GenerateRefreshToken();
        public DateTime GetRefreshTokenExpirationTime();
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken);
    }
}
