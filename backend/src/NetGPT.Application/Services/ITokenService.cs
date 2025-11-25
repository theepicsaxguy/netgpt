// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Security.Claims;

namespace NetGPT.Application.Services
{
    public interface ITokenService
    {
        string CreateAccessToken(object user, TimeSpan? lifetime = null);

        (string token, DateTime expiresAt) CreateRefreshToken();

        ClaimsPrincipal? ValidateAccessToken(string token, bool validateLifetime = true);

        string HashRefreshToken(string token);
    }
}