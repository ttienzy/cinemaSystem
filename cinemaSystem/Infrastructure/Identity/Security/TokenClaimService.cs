using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Security;
using Application.Settings;
using Infrastructure.Redis.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Models.IdentityModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Infrastructure.Identity.Security
{
    public class TokenClaimService : ITokenClaimService
    {
        private readonly ICacheService _cacheService;
        private readonly JwtSettings _jwtSettings;
        private const int RefreshTokenLenght = 32;
        public TokenClaimService(IOptions<JwtSettings> options, ICacheService cacheService)
        {
            _jwtSettings = options.Value ?? throw new ArgumentNullException(nameof(options), "JwtSettings cannot be null");
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService), "CacheService cannot be null");
        }
        public string GenerateAccessTokenn(Guid userId, string userName, string email, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtSettings.AccessTokenExpiration)),
                SigningCredentials = signingCredentials,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public string GenerateRefreshToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var randomBytes = new byte[RefreshTokenLenght];
                rng.GetBytes(randomBytes);
                return Base64UrlEncode(randomBytes);
            }
        }
        private string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Replace('+', '-'); // Thay thế '+' bằng '-'
            output = output.Replace('/', '_'); // Thay thế '/' bằng '_'
            output = output.Replace("=", "");  // Xóa ký tự đệm '='
            return output;
        }

        public DateTime GetRefreshTokenExpirationTime()
        {
            return DateTime.UtcNow.AddDays(Convert.ToDouble(_jwtSettings.RefreshTokenExpiration));
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token format.");
                }

                return principal;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                throw new SecurityTokenException("Invalid token signature.");
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Invalid token format: {ex.Message}", nameof(token));
            }
            catch (Exception ex) when (ex is SecurityTokenException || ex is InvalidOperationException)
            {
                throw new SecurityTokenException($"Token validation failed: {ex.Message}");
            }
        }

        public async Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken)
        {
            var cachedToken = await _cacheService.GetAsync<RefreshTokenModel>(CacheKey.RefreshToken(userId));

            if (cachedToken == null)
            {
                return false; // Refresh token not found in cache
            }
            if (cachedToken.RefreshToken != refreshToken)
            {
                await _cacheService.RemoveAsync(CacheKey.RefreshToken(userId));
                return false; // Refresh token does not match
            }
            if (cachedToken.Expiration < DateTime.UtcNow)
            {
                await _cacheService.RemoveAsync(CacheKey.RefreshToken(userId));
                return false; // Refresh token has expired
            }
            return true;
        }

        /// <summary>
        /// Hủy refresh token — xóa khỏi Redis cache, buộc user phải đăng nhập lại.
        /// </summary>
        public async Task RevokeRefreshTokenAsync(Guid userId)
        {
            await _cacheService.RemoveAsync(CacheKey.RefreshToken(userId));
        }
    }
}
