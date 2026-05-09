using Cinema.Shared.Constants;
using Cinema.Shared.Helpers;
using Cinema.Shared.Models;
using Identity.API.Infrastructure.Persistence;
using Identity.API.Application.DTOs;
using Identity.API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Identity.API.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IdentityDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IdentityDbContext context,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<string>.FailureResponse(
                "Email already exists",
                400,
                new List<ErrorDetail>
                {
                    new ErrorDetail("EMAIL_EXISTS", "This email is already registered", "Email")
                }
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
                "Registration failed",
                400,
                errors
            );
        }

        await _userManager.AddToRoleAsync(user, AppConstants.Roles.Customer);

        return ApiResponse<string>.SuccessResponse(
            user.Id,
            "Registration successful",
            201
        );
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse("Invalid email or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse("Invalid email or password");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var accessToken = JwtHelper.GenerateToken(
            user.Id,
            user.Email!,
            roles.ToArray(),
            _jwtSettings.SecretKey,
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            _jwtSettings.ExpirationMinutes
        );

        var refreshToken = GenerateRefreshToken();
        await SaveRefreshTokenAsync(user.Id, refreshToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName ?? string.Empty,
            Roles = roles.ToList(),
            ExpiresAt = expiresAt
        };

        return ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful");
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse("Invalid refresh token");
        }

        if (storedToken.IsRevoked)
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse("Refresh token has been revoked");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return ApiResponse<LoginResponse>.UnauthorizedResponse("Refresh token has expired");
        }

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var accessToken = JwtHelper.GenerateToken(
            user.Id,
            user.Email!,
            roles.ToArray(),
            _jwtSettings.SecretKey,
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            _jwtSettings.ExpirationMinutes
        );

        var newRefreshToken = GenerateRefreshToken();
        storedToken.IsRevoked = true;
        storedToken.ReplacedByToken = newRefreshToken;
        await SaveRefreshTokenAsync(user.Id, newRefreshToken);
        await _context.SaveChangesAsync();

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName ?? string.Empty,
            Roles = roles.ToList(),
            ExpiresAt = expiresAt
        };

        return ApiResponse<LoginResponse>.SuccessResponse(response, "Token refreshed successfully");
    }

    public async Task<ApiResponse<UserInfoResponse>> GetUserInfoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserInfoResponse>.NotFoundResponse("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var userInfo = new UserInfoResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList()
        };

        return ApiResponse<UserInfoResponse>.SuccessResponse(userInfo, "User info retrieved successfully");
    }

    public async Task<ApiResponse<bool>> RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
        {
            return ApiResponse<bool>.NotFoundResponse("Refresh token not found");
        }

        if (storedToken.IsRevoked)
        {
            return ApiResponse<bool>.FailureResponse("Token already revoked", 400);
        }

        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Token revoked successfully");
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task SaveRefreshTokenAsync(string userId, string token)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }
}


