using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cinema.Shared.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Application.Services;

public class AccessTokenService : IAccessTokenService
{
    private readonly JwtSettings _jwtSettings;

    public AccessTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: GetExpirationTime(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpirationTime()
    {
        return DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
    }
}
