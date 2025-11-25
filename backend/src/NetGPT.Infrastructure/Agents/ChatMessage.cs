// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Agents
{
    /// <summary>
    /// Lightweight chat message DTO used across the infrastructure to avoid direct dependency
    /// on SDK types in application code. This mirrors minimal role/content fields.
    /// </summary>
    internal sealed class ChatMessage
    {
        public string? Role { get; set; }

        public string? Content { get; set; }
    }
}
