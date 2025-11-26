// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using Microsoft.AspNetCore.Http;

namespace NetGPT.API.Controllers
{
    /// <summary>
    /// Contains helper methods for authentication operations in <see cref="AuthController"/>.
    /// </summary>
    public sealed partial class AuthController
    {
        private void SetRefreshCookie(string token, DateTime expiresAt)
        {
            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = !string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase),
                SameSite = SameSiteMode.Strict,
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
