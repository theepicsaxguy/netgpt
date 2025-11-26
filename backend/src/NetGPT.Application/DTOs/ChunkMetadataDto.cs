// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Application.DTOs
{
    /// <summary>
    /// Metadata associated with a streaming chunk.
    /// </summary>
    public record ChunkMetadataDto(
        string? EventType = null,
        string? ToolName = null,
        string? ToolInvocationId = null,
        string? ToolArguments = null,
        string? ToolResult = null,
        DateTime? Timestamp = null);
}
