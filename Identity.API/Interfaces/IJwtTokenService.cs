using Identity.API.Models;

namespace Identity.API.Interfaces
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IList<string> roles);

    }
}
