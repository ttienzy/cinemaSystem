namespace Identity.API.Application.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken?> GetByTokenWithUserAsync(string token);
    Task<string> IssueAsync(string userId);
    Task<string> RotateAsync(RefreshToken refreshToken);
    Task RevokeAsync(RefreshToken refreshToken);
}
