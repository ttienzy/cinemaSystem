using Cinema.Shared.Constants;
using Cinema.Shared.Models;
using Identity.API.Application.DTOs;
using Identity.API.Application.Mappers;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<string>.FailureResponse(
                AuthException.EMAIL_ALREADY_EXISTS,
                400,
                [AuthException.EmailAlreadyExists()]
            );
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e =>
                new ErrorDetail("VALIDATION_ERROR", e.Description, e.Code)
            ).ToList();

            return ApiResponse<string>.FailureResponse(
                AuthException.REGISTRATION_FAILED,
                400,
                errors
            );
        }

        await _userManager.AddToRoleAsync(user, AppConstants.Roles.Customer);

        return ApiResponse<string>.SuccessResponse(
            user.Id,
            AuthException.REGISTRATION_SUCCESSFUL,
            201
        );
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse(AuthException.INVALID_EMAIL_OR_PASSWORD);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse(AuthException.INVALID_EMAIL_OR_PASSWORD);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var expiresAt = _accessTokenService.GetExpirationTime();
        var accessToken = _accessTokenService.GenerateToken(user, roles);
        var refreshToken = await _refreshTokenService.IssueAsync(user.Id);
        var response = user.ToLoginResponse(roles, accessToken, refreshToken, expiresAt);

        return ApiResponse<LoginResponse>.SuccessResponse(response, AuthException.LOGIN_SUCCESSFUL);
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenService.GetByTokenWithUserAsync(refreshToken);

        if (storedToken == null)
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse(AuthException.INVALID_REFRESH_TOKEN);
        }

        if (storedToken.IsRevoked)
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse(AuthException.REFRESH_TOKEN_REVOKED);
        }

        if (storedToken.IsExpired(DateTime.UtcNow))
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse(AuthException.REFRESH_TOKEN_EXPIRED);
        }

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);
        var expiresAt = _accessTokenService.GetExpirationTime();
        var accessToken = _accessTokenService.GenerateToken(user, roles);
        var newRefreshToken = await _refreshTokenService.RotateAsync(storedToken);
        var response = user.ToLoginResponse(roles, accessToken, newRefreshToken, expiresAt);

        return ApiResponse<LoginResponse>.SuccessResponse(response, AuthException.TOKEN_REFRESHED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<UserInfoResponse>> GetUserInfoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserInfoResponse>.NotFoundResponse(AuthException.USER_NOT_FOUND);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = user.ToUserInfoResponse(roles);

        return ApiResponse<UserInfoResponse>.SuccessResponse(userInfo, AuthException.USER_INFO_RETRIEVED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<bool>> RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenService.GetByTokenWithUserAsync(refreshToken);

        if (storedToken == null)
        {
            return ApiResponse<bool>.NotFoundResponse(AuthException.REFRESH_TOKEN_NOT_FOUND);
        }

        if (storedToken.IsRevoked)
        {
            return ApiResponse<bool>.FailureResponse(AuthException.TOKEN_ALREADY_REVOKED, 400);
        }

        await _refreshTokenService.RevokeAsync(storedToken);

        return ApiResponse<bool>.SuccessResponse(true, AuthException.TOKEN_REVOKED_SUCCESSFULLY);
    }
}


