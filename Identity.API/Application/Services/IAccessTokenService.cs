namespace Identity.API.Application.Services;

public interface IAccessTokenService
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
    DateTime GetExpirationTime();
}
