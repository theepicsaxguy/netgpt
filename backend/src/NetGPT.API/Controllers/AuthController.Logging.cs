// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using Microsoft.Extensions.Logging;

namespace NetGPT.API.Controllers
{
    public sealed partial class AuthController
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Login issued refresh token for user {UserId}")]
        private static partial void LogLoginIssuedRefreshToken(ILogger logger, Guid userId);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "User registered: {Username}")]
        private static partial void LogUserRegistered(ILogger logger, string username);

        [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Refresh failed: token not found")]
        private static partial void LogRefreshTokenNotFound(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Refresh token replay detected for user {UserId}")]
        private static partial void LogRefreshTokenReplayDetected(ILogger logger, Guid userId);

        [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Refresh failed: token expired or revoked for user {UserId}")]
        private static partial void LogRefreshTokenExpiredOrRevoked(ILogger logger, Guid userId);

        [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Logout: revoked refresh token for user {UserId}")]
        private static partial void LogLogoutRevokedRefreshToken(ILogger logger, Guid userId);
    }
}
