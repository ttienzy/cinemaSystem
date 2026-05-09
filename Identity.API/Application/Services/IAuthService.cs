using Cinema.Shared.Models;
using Identity.API.Application.DTOs;

namespace Identity.API.Application.Services;

public interface IAuthService
{
    Task<ApiResponse<string>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<UserInfoResponse>> GetUserInfoAsync(string userId);
    Task<ApiResponse<bool>> RevokeTokenAsync(string refreshToken);
}


