// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs.Auth
{
    public sealed class RegisterRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
}
