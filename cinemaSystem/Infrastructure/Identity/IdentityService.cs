using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Application.Interfaces.Security;
using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Identity;
using Shared.Common.Base;
using Shared.EmailTemplates;
using Shared.Models.IdentityModels;
using Shared.Models.IdentityModels.Otps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public class IdentityService : IIdentityUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ITokenClaimService _tokenClaimService;
        private readonly ICacheService _cacheService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ITokenClaimService tokenClaimService,
            ICacheService cacheService,
            IOtpService otpService,
            IEmailService emailService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _tokenClaimService = tokenClaimService ?? throw new ArgumentNullException(nameof(tokenClaimService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
            _emailService = emailService ?? throw new AggregateException(nameof(emailService));
        }

        public async Task<BaseResponse<string>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("User not found."));
                }

                var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BaseResponse<string>.Failure(Error.BadRequest(errors));
                }

                return BaseResponse<string>.Success("Password changed successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("User not found."));
                }

                var otpCode = await _otpService.GenerateOtpAsync(user.Id.ToString(), user.Email!);
                var emailRequest = PasswordResetTemplates.VerificationCode(user.Email!, otpCode);
                await _emailService.SendEmailAsync(emailRequest);
                return BaseResponse<string>.Success("Verification code sent to your email.");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<UserProfileResponse>> GetUserProfileAsync(Guid userId)
        {
            try
            {
                var userProfile = await _userManager.FindByIdAsync(userId.ToString());
                if (userProfile == null)
                {
                    return BaseResponse<UserProfileResponse>.Failure(Error.NotFound("User profile not found."));
                }

#pragma warning disable CS8601 // Possible null reference assignment.
                var response = new UserProfileResponse
                {
                    UserId = userProfile.Id,
                    UserName = userProfile.UserName,
                    Email = userProfile.Email,
                    PhoneNumber = userProfile.PhoneNumber,
                    Role = (await _userManager.GetRolesAsync(userProfile)).FirstOrDefault() ?? string.Empty
                };
#pragma warning restore CS8601 // Possible null reference assignment.

                return BaseResponse<UserProfileResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<UserProfileResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<LoginResponse>> LoginUserAsync(LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BaseResponse<LoginResponse>.Failure(Error.NotFound("User not found."));
                }

                var result = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!result)
                {
                    return BaseResponse<LoginResponse>.Failure(Error.Unauthorized("Invalid password."));
                }
                var roles = await _userManager.GetRolesAsync(user);
                // Generate token using the token claim service
                var accessToken = _tokenClaimService.GenerateAccessTokenn(user.Id, user.Email!, roles.ToList());
                var refreshToken = _tokenClaimService.GenerateRefreshToken();
                var refreshTokenExpiry = _tokenClaimService.GetRefreshTokenExpirationTime();
                var userResponse = new UserProfileResponse
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? string.Empty
                };
                var tokenResponse = new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                };

                // Store the refresh token in cache with expiration
                await _cacheService.SetAsync($"refresh_token_{user.Id}", new RefreshTokenModel { RefreshToken = refreshToken, Expiration = refreshTokenExpiry }, refreshTokenExpiry - DateTime.UtcNow);

                return BaseResponse<LoginResponse>.Success(new LoginResponse
                {
                    UserProfile = userResponse,
                    Token = tokenResponse
                });
            }
            catch (Exception ex)
            {
                return BaseResponse<LoginResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                };
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BaseResponse<string>.Failure(Error.BadRequest(errors));
                }
                //Set default role if it exists
                if (await _roleManager.RoleExistsAsync(RoleConstant.User))
                {
                    await _userManager.AddToRoleAsync(user, RoleConstant.User);
                }

                return BaseResponse<string>.Success("User registered successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> ResendOtpAsync(string email)
        {
            try
            {
                await ForgotPasswordAsync(email);
                return BaseResponse<string>.Success("OTP resent successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordWithOtpRequest request)
        {
            try
            {
                var pass_reset_otp = await _otpService.ValidateOtpAsync(request.Email, request.OtpCode);
                if (!pass_reset_otp)
                {
                    return BaseResponse<string>.Failure(Error.BadRequest("Invalid or expired OTP."));
                }
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("User not found."));
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);


                if (!resetPasswordResult.Succeeded)
                {
                    var errors = string.Join(", ", resetPasswordResult.Errors.Select(e => e.Description));
                    return BaseResponse<string>.Failure(Error.BadRequest(errors));
                }
                // Mark the OTP as used, remove it from cache
                await _cacheService.RemoveAsync($"password_reset_otp_{request.Email.ToLower()}");

                return BaseResponse<string>.Success("Password reset successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("User not found."));
                }
                user.UserName = request.UserName ?? user.UserName;
                user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
                // Update the user profile
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BaseResponse<string>.Failure(Error.BadRequest(errors));
                }
                // Return the updated profile
                return BaseResponse<string>.Success("Profile updated successfully.");

            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<string>> VerifyResetOtpAsync(VerifyResetOtpRequest request)
        {
            try
            {
                var isValidOtp = await _otpService.ValidateOtpAsync(request.Email, request.OtpCode);
                if (!isValidOtp)
                {
                    return BaseResponse<string>.Failure(Error.BadRequest("Invalid or expired OTP."));
                }
                // If OTP is valid, return success response
                return BaseResponse<string>.Success("OTP verified successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
