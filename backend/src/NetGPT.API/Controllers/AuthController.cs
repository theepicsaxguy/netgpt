// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NetGPT.Application.DTOs.Auth;
using NetGPT.Application.Services;
using NetGPT.Infrastructure.Persistence.Entities;
using NetGPT.Infrastructure.Persistence.Repositories;

namespace NetGPT.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly ITokenService tokenService;
        private readonly RefreshTokenRepository refreshRepo;
        private readonly IConfiguration configuration;
        private readonly Microsoft.Extensions.Logging.ILogger<AuthController> logger;

        public AuthController(ITokenService tokenService, RefreshTokenRepository refreshRepo, IConfiguration configuration, Microsoft.Extensions.Logging.ILogger<AuthController> logger)
        {
            this.tokenService = tokenService;
            this.refreshRepo = refreshRepo;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Deterministic user id derived from username (useful for testing).
            Guid userId = DeriveDeterministicGuid(request.Username);

            // Build a minimal user object for TokenService to include name and roles claims
            var userObj = new { Id = userId, Name = request.Username, Roles = Array.Empty<string>() };

            string accessToken = tokenService.CreateAccessToken(userObj);
            var (refreshTokenPlain, refreshExpiresAt) = tokenService.CreateRefreshToken();
            string refreshHash = tokenService.HashRefreshToken(refreshTokenPlain);

            var refreshEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = refreshHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshExpiresAt,
            };

            await refreshRepo.AddAsync(refreshEntity);
            await refreshRepo.SaveChangesAsync();

            logger.LogInformation("Login: issued refresh token for user {UserId}", userId);

            SetRefreshCookie(refreshTokenPlain, refreshExpiresAt);

            return Ok(new AccessTokenResponseDto { AccessToken = accessToken, ExpiresAt = DateTime.UtcNow.AddMinutes(15) });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refresh_token", out string? refreshTokenPlain) || string.IsNullOrWhiteSpace(refreshTokenPlain))
            {
                ClearRefreshCookie();
                return Unauthorized();
            }

            string hash = tokenService.HashRefreshToken(refreshTokenPlain);
            var existing = await refreshRepo.GetByHashAsync(hash);
            if (existing == null)
            {
                logger.LogWarning("Refresh failed: token not found");
                ClearRefreshCookie();
                return Unauthorized();
            }

            // Replay detection: token has already been rotated
            if (existing.ReplacedByTokenId != null)
            {
                logger.LogWarning("Refresh token replay detected for user {UserId}. Revoking all tokens.", existing.UserId);
                await refreshRepo.RevokeAllForUserAsync(existing.UserId);
                await refreshRepo.SaveChangesAsync();
                ClearRefreshCookie();
                return Unauthorized();
            }

            if (existing.ExpiresAt <= DateTime.UtcNow || existing.RevokedAt != null)
            {
                logger.LogWarning("Refresh failed: token expired or revoked for user {UserId}", existing.UserId);
                ClearRefreshCookie();
                return Unauthorized();
            }

            // Rotate
            var (newRefreshPlain, newExpiresAt) = tokenService.CreateRefreshToken();
            string newHash = tokenService.HashRefreshToken(newRefreshPlain);

            var newEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = existing.UserId,
                TokenHash = newHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = newExpiresAt,
            };

            await refreshRepo.AddAsync(newEntity);
            await refreshRepo.RevokeAsync(existing, newEntity.Id);
            await refreshRepo.SaveChangesAsync();

            var userObj = new { Id = existing.UserId, Name = existing.UserId.ToString(), Roles = Array.Empty<string>() };
            string accessToken = tokenService.CreateAccessToken(userObj);
            SetRefreshCookie(newRefreshPlain, newExpiresAt);

            return Ok(new AccessTokenResponseDto { AccessToken = accessToken, ExpiresAt = DateTime.UtcNow.AddMinutes(15) });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refresh_token", out string? refreshTokenPlain) && !string.IsNullOrWhiteSpace(refreshTokenPlain))
            {
                string hash = tokenService.HashRefreshToken(refreshTokenPlain);
                var existing = await refreshRepo.GetByHashAsync(hash);
                if (existing != null)
                {
                    await refreshRepo.RevokeAsync(existing, null);
                    await refreshRepo.SaveChangesAsync();
                    logger.LogInformation("Logout: revoked refresh token for user {UserId}", existing.UserId);
                }
            }

            ClearRefreshCookie();
            return Ok();
        }

        private void SetRefreshCookie(string token, DateTime expiresAt)
        {
            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = !string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase),
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                Expires = expiresAt
            };
            Response.Cookies.Append("refresh_token", token, cookieOptions);
        }

        private void ClearRefreshCookie()
        {
            Response.Cookies.Delete("refresh_token");
        }

        private static Guid DeriveDeterministicGuid(string input)
        {
            // Use SHA-1 and take first 16 bytes for a deterministic GUID from username
            using var sha = System.Security.Cryptography.SHA1.Create();
            byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input ?? string.Empty));
            byte[] guidBytes = new byte[16];
            Array.Copy(hash, guidBytes, 16);
            return new Guid(guidBytes);
        }
    }
}
