using System.Security.Cryptography;
using Identity.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IdentityDbContext _context;

    public RefreshTokenService(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenWithUserAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == token);
    }

    public async Task<string> IssueAsync(string userId)
    {
        var token = GenerateRefreshTokenValue();
        var refreshToken = RefreshToken.Create(userId, token, DateTime.UtcNow.Add(RefreshTokenLifetime));

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task<string> RotateAsync(RefreshToken refreshToken)
    {
        var newToken = GenerateRefreshTokenValue();
        refreshToken.Revoke(newToken);

        _context.RefreshTokens.Add(
            RefreshToken.Create(refreshToken.UserId, newToken, DateTime.UtcNow.Add(RefreshTokenLifetime)));

        await _context.SaveChangesAsync();
        return newToken;
    }

    public async Task RevokeAsync(RefreshToken refreshToken)
    {
        refreshToken.Revoke();
        await _context.SaveChangesAsync();
    }

    private static string GenerateRefreshTokenValue()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
