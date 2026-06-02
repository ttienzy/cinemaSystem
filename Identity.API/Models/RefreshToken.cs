namespace Identity.API.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set;  } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
