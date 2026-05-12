using Cinema.Shared.Entities;

namespace Identity.API.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }

    public static RefreshToken Create(string userId, string token, DateTime expiresAt)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public bool IsExpired(DateTime now)
    {
        return ExpiresAt <= now;
    }

    public bool CanBeUsed(DateTime now)
    {
        return !IsRevoked && !IsExpired(now);
    }

    public void Revoke(string? replacedByToken = null)
    {
        IsRevoked = true;
        ReplacedByToken = replacedByToken;
        UpdatedAt = DateTime.UtcNow;
    }
}


