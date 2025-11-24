// <copyright file="StreamingChunk.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System;

    public record StreamingChunkDto(
        Guid MessageId,
        string? Content,
        ToolInvocationDto? ToolInvocation = null,
        bool IsComplete = false);
}
