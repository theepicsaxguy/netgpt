// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    using System;

    public record StreamingChunkDto(
        Guid MessageId,
        string? Content,
        ToolInvocationDto? ToolInvocation = null,
        bool IsComplete = false);
}
