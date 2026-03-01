using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Security;
using Infrastructure.Identity.Constants;
using Infrastructure.Redis.Constants;
using Microsoft.AspNetCore.Identity;
using Shared.Models.IdentityModels;
using Shared.Models.IdentityModels.Otps;
using Shared.EmailTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Templates;
using Shared.Models.ExtenalModels;

namespace Infrastructure.Identity
{
    public class IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ITokenClaimService tokenClaimService,
        ICacheService cacheService,
        IOtpService otpService,
        IEmailService emailService) : IIdentityUserService
    {
        public async Task ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email)
                ?? throw new NotFoundException("User", request.Email);

            var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException("Password", errors);
            }
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email)
                ?? throw new NotFoundException("User", email);

            var otpCode = await otpService.GenerateOtpAsync(user.Id.ToString(), user.Email!);
            var emailRequest = PasswordResetTemplates.VerificationCode(user.Email!, otpCode);
            await emailService.SendEmailAsync(emailRequest);
        }

        public async Task<UserProfileResponse> GetUserProfileAsync(Guid userId)
        {
            var userProfile = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("User", userId);

            return new UserProfileResponse
            {
                UserName = userProfile.UserName ?? string.Empty,
                Email = userProfile.Email ?? string.Empty,
                PhoneNumber = userProfile.PhoneNumber,
                Roles = (await userManager.GetRolesAsync(userProfile)).ToList(),
                CreatedAt = userProfile.CreatedAt
            };
        }

        public async Task<LoginResponse> LoginUserAsync(LoginRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email)
                ?? throw new UnauthorizedException("Invalid email or password.");

            var result = await userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
                throw new UnauthorizedException("Invalid email or password.");

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = tokenClaimService.GenerateAccessTokenn(user.Id, user.UserName ?? string.Empty, user.Email!, roles.ToList());
            var refreshToken = tokenClaimService.GenerateRefreshToken();
            var refreshTokenExpiry = tokenClaimService.GetRefreshTokenExpirationTime();

            await cacheService.SetAsync(
                CacheKey.RefreshToken(user.Id), 
                new RefreshTokenModel { RefreshToken = refreshToken, Expiration = refreshTokenExpiry }, 
                refreshTokenExpiry - DateTime.UtcNow);

            return new LoginResponse
            {
                Token = new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }
            };
        }

        public async Task RegisterUserAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException("User", errors);
            }

            if (await roleManager.RoleExistsAsync(RoleConstant.Staff))
            {
                await userManager.AddToRoleAsync(user, RoleConstant.Staff);
            }
        }

        public async Task ResendOtpAsync(string email)
        {
            await ForgotPasswordAsync(email);
        }

        public async Task ResetPasswordAsync(ResetPasswordWithOtpRequest request)
        {
            var isValid = await otpService.ValidateOtpAsync(request.Email, request.OtpCode);
            if (!isValid)
                throw new ValidationException("OtpCode", "Invalid or expired OTP.");

            var user = await userManager.FindByEmailAsync(request.Email)
                ?? throw new NotFoundException("User", request.Email);

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException("Password", errors);
            }

            await cacheService.RemoveAsync($"password_reset_otp_{request.Email.ToLower()}");
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("User", userId);

            user.UserName = request.UserName ?? user.UserName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException("Profile", errors);
            }
        }

        public async Task CreateStaffAsync(CreateStaffRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true 
            };

            var temporaryPassword = $"Staff@{Guid.NewGuid().ToString("N")[..8]}!";
            var result = await userManager.CreateAsync(user, temporaryPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException("User", errors);
            }

            if (await roleManager.RoleExistsAsync(request.Role))
            {
                await userManager.AddToRoleAsync(user, request.Role);
            }
            else
            {
                throw new ValidationException("Role", $"Role {request.Role} does not exist.");
            }

            // Gửi email thông báo
            var emailRequest = StaffTemplates.WelcomeStaff(user.Email, user.UserName, temporaryPassword, request.Role);
            await emailService.SendEmailAsync(emailRequest);
        }

        public async Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request)
        {
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("User", userId);

            if (!await roleManager.RoleExistsAsync(request.RoleName))
                throw new ValidationException("RoleName", $"Role {request.RoleName} does not exist.");

            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, request.RoleName);
        }

        public async Task<bool> VerifyResetOtpAsync(VerifyResetOtpRequest request)
        {
            var isValid = await otpService.ValidateOtpAsync(request.Email, request.OtpCode);
            if (!isValid)
                throw new ValidationException("OtpCode", "Invalid or expired OTP.");

            return true;
        }
    }
}
