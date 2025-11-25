// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Infrastructure.Persistence.Entities
{
    public sealed class RefreshToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public Guid? ReplacedByTokenId { get; set; }

        public string? DeviceFingerprint { get; set; }
    }
}
