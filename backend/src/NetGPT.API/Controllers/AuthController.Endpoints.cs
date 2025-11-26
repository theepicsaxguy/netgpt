// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetGPT.Application.DTOs.Auth;
using NetGPT.Domain.Entities;
using NetGPT.Infrastructure.Persistence.Entities;

namespace NetGPT.API.Controllers
{
    /// <summary>
    /// Partial implementations for `AuthController` endpoints.
    /// </summary>
    public sealed partial class AuthController
    {
        [HttpPost("login")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest();
            }

            User? user = await userRepo.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!hasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized();
            }

            string[] roles = (user.Roles ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
            object userObj = new { user.Id, Name = user.Name ?? user.Username, Roles = roles };

            string accessToken = tokenService.CreateAccessToken(userObj);
            (string refreshTokenPlain, DateTime refreshExpiresAt) = tokenService.CreateRefreshToken();
            string refreshHash = tokenService.HashRefreshToken(refreshTokenPlain);

            RefreshToken refreshEntity = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshExpiresAt,
            };

            await refreshRepo.AddAsync(refreshEntity);
            _ = await refreshRepo.SaveChangesAsync();

            LogLoginIssuedRefreshToken(logger, user.Id);

            SetRefreshCookie(refreshTokenPlain, refreshExpiresAt);

            return Ok(new AccessTokenResponseDto { AccessToken = accessToken, ExpiresAt = DateTime.UtcNow.AddMinutes(15) });
        }

        [HttpPost("register")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest();
            }

            User? existing = await userRepo.GetByUsernameAsync(request.Username);
            if (existing != null)
            {
                return Conflict();
            }

            hasher.CreateHash(request.Password, out byte[] hash, out byte[] salt);

            User user = new()
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
            };

            _ = await userRepo.AddAsync(user);
            _ = await refreshRepo.SaveChangesAsync();

            LogUserRegistered(logger, user.Username);

            return Ok();
        }

        [HttpPost("refresh")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
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
                LogRefreshTokenNotFound(logger);
                ClearRefreshCookie();
                return Unauthorized();
            }

            if (existing.ReplacedByTokenId != null)
            {
                LogRefreshTokenReplayDetected(logger, existing.UserId);
                await refreshRepo.RevokeAllForUserAsync(existing.UserId);
                _ = await refreshRepo.SaveChangesAsync();
                ClearRefreshCookie();
                return Unauthorized();
            }

            if (existing.ExpiresAt <= DateTime.UtcNow || existing.RevokedAt != null)
            {
                LogRefreshTokenExpiredOrRevoked(logger, existing.UserId);
                ClearRefreshCookie();
                return Unauthorized();
            }

            (string newRefreshPlain, DateTime newExpiresAt) = tokenService.CreateRefreshToken();
            string newHash = tokenService.HashRefreshToken(newRefreshPlain);

            RefreshToken newEntity = new()
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
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
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
                    LogLogoutRevokedRefreshToken(logger, existingLogout.UserId);
                }
            }

            ClearRefreshCookie();
            return Ok();
        }
    }
}
