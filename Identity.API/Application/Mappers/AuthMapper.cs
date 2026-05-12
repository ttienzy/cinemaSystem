namespace Identity.API.Application.Mappers;

public static class AuthMapper
{
    public static LoginResponse ToLoginResponse(
        this ApplicationUser user,
        IEnumerable<string> roles,
        string accessToken,
        string refreshToken,
        DateTime expiresAt)
    {
        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Roles = roles.ToList(),
            ExpiresAt = expiresAt
        };
    }

    public static UserInfoResponse ToUserInfoResponse(this ApplicationUser user, IEnumerable<string> roles)
    {
        return new UserInfoResponse
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList()
        };
    }
}
