using Cinema.Shared.Models;

namespace Identity.API.Domain.Exceptions;

public static class AuthException
{
    public const string EMAIL_ALREADY_EXISTS = "Email already exists";
    public const string REGISTRATION_FAILED = "Registration failed";
    public const string REGISTRATION_SUCCESSFUL = "Registration successful";
    public const string INVALID_EMAIL_OR_PASSWORD = "Invalid email or password";
    public const string LOGIN_SUCCESSFUL = "Login successful";
    public const string INVALID_REFRESH_TOKEN = "Invalid refresh token";
    public const string REFRESH_TOKEN_REVOKED = "Refresh token has been revoked";
    public const string REFRESH_TOKEN_EXPIRED = "Refresh token has expired";
    public const string TOKEN_REFRESHED_SUCCESSFULLY = "Token refreshed successfully";
    public const string USER_NOT_FOUND = "User not found";
    public const string USER_INFO_RETRIEVED_SUCCESSFULLY = "User info retrieved successfully";
    public const string REFRESH_TOKEN_NOT_FOUND = "Refresh token not found";
    public const string TOKEN_ALREADY_REVOKED = "Token already revoked";
    public const string TOKEN_REVOKED_SUCCESSFULLY = "Token revoked successfully";

    public static ErrorDetail EmailAlreadyExists()
    {
        return new ErrorDetail("EMAIL_EXISTS", "This email is already registered", "Email");
    }
}
