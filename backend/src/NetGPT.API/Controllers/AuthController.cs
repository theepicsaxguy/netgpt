// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly NetGPT.Application.Interfaces.IUserRepository userRepo;
        private readonly NetGPT.Infrastructure.Services.IPasswordHasher hasher;

        public AuthController(ITokenService tokenService, RefreshTokenRepository refreshRepo, IConfiguration configuration, Microsoft.Extensions.Logging.ILogger<AuthController> logger, NetGPT.Application.Interfaces.IUserRepository userRepo, NetGPT.Infrastructure.Services.IPasswordHasher hasher)
        {
            this.tokenService = tokenService;
            this.refreshRepo = refreshRepo;
            this.configuration = configuration;
            this.logger = logger;
            this.userRepo = userRepo;
            this.hasher = hasher;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest();

            var user = await userRepo.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                // user does not exist
                return Unauthorized();
            }

            if (!hasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized();
            }

            // Build user object to include claims
            string[] roles = string.IsNullOrEmpty(user.Roles) ? Array.Empty<string>() : user.Roles.Split(',');
            object userObj = new { Id = user.Id, Name = user.Name ?? user.Username, Roles = roles };

            string accessToken = tokenService.CreateAccessToken(userObj);
            (string refreshTokenPlain, DateTime refreshExpiresAt) = tokenService.CreateRefreshToken();
            string refreshHash = tokenService.HashRefreshToken(refreshTokenPlain);

            RefreshToken refreshEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshExpiresAt,
            };

            await refreshRepo.AddAsync(refreshEntity);
            _ = await refreshRepo.SaveChangesAsync();

            logger.LogInformation("Login: issued refresh token for user {UserId}", user.Id);

            SetRefreshCookie(refreshTokenPlain, refreshExpiresAt);

            return Ok(new AccessTokenResponseDto { AccessToken = accessToken, ExpiresAt = DateTime.UtcNow.AddMinutes(15) });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] NetGPT.Application.DTOs.Auth.RegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest();
            }

            var existing = await userRepo.GetByUsernameAsync(request.Username);
            if (existing != null)
            {
                return Conflict();
            }

            hasher.CreateHash(request.Password, out byte[] hash, out byte[] salt);

            NetGPT.Infrastructure.Persistence.Entities.User user = new NetGPT.Infrastructure.Persistence.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
            };

            await userRepo.AddAsync(user);
            _ = await refreshRepo.SaveChangesAsync();

            logger.LogInformation("User registered: {Username}", user.Username);

            return Ok();
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
            RefreshToken? existing = await refreshRepo.GetByHashAsync(hash);
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
            (string newRefreshPlain, DateTime newExpiresAt) = tokenService.CreateRefreshToken();
            string newHash = tokenService.HashRefreshToken(newRefreshPlain);

            RefreshToken newEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = existing.UserId,
                TokenHash = newHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = newExpiresAt,
            };

            await refreshRepo.AddAsync(newEntity);
            await refreshRepo.RevokeAsync(existing, newEntity.Id);
            _ = await refreshRepo.SaveChangesAsync();

            object userObj = new { Id = existing.UserId, Name = existing.UserId.ToString(), Roles = Array.Empty<string>() };
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
                RefreshToken? existingLogout = await refreshRepo.GetByHashAsync(hash);
                if (existingLogout != null)
                {
                    await refreshRepo.RevokeAsync(existingLogout, null);
                    _ = await refreshRepo.SaveChangesAsync();
                    logger.LogInformation("Logout: revoked refresh token for user {UserId}", existingLogout.UserId);
                }
            }

            ClearRefreshCookie();
            return Ok();
        }

        private void SetRefreshCookie(string token, DateTime expiresAt)
        {
            Microsoft.AspNetCore.Http.CookieOptions cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = !string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase),
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                Expires = expiresAt,
            };
            Response.Cookies.Append("refresh_token", token, cookieOptions);
        }

        private void ClearRefreshCookie()
        {
            Response.Cookies.Delete("refresh_token");
        }

        // (Removed deterministic GUID helper — user registration now uses real users table)
    }
}
