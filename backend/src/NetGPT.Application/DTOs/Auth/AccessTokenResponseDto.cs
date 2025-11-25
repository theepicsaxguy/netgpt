// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs.Auth
{
    public sealed class AccessTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
