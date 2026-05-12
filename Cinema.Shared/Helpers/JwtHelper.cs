using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Cinema.Shared.Helpers;

public static class JwtHelper
{
    public static ClaimsPrincipal? ValidateToken(string token, string secretKey, string issuer, string audience)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }

    public static string? GetUserId(ClaimsPrincipal principal)
    {
        return GetClaimValue(principal, ClaimTypes.NameIdentifier);
    }

    public static string? GetEmail(ClaimsPrincipal principal)
    {
        return GetClaimValue(principal, ClaimTypes.Email);
    }

    public static IEnumerable<string> GetRoles(ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    public static JwtSecurityToken? DecodeToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }
        catch
        {
            return null;
        }
    }
}
