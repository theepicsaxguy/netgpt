// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    /// <summary>
    /// Represents a tool parameter.
    /// </summary>
    public record ToolParameterDto(
        string Name,
        string Type,
        string? Description,
        bool IsRequired);
}
