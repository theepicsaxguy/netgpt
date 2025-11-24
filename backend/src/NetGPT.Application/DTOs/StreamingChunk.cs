
// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Application.DTOs
{
    public record StreamingChunkDto(
        Guid MessageId,
        string? Content,
        ToolInvocationDto? ToolInvocation = null,
        bool IsComplete = false);
}
