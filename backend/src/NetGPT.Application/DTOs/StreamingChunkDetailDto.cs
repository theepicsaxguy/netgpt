// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Application.DTOs
{
    public record StreamingChunkDetailDto(
        Guid Id,
        Guid? MessageId,
        string Content,
        bool IsComplete,
        DateTime EmittedAt);
}