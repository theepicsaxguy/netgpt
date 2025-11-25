// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Application.DTOs
{
    public record StreamingChunkDto(
        Guid ChunkId,
        string Text,
        bool IsFinal,
        DateTime CreatedAt);
}
