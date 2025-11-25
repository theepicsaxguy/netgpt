// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Infrastructure.Persistence.Entities
{
    public sealed class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        public string? Name { get; set; }

        public string? Roles { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
