// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;

namespace NetGPT.Application.DTOs
{
    /// <summary>
    /// Represents metadata about a registered tool.
    /// </summary>
    public record ToolInfoDto(
        string Name,
        string? Description,
        List<ToolParameterDto> Parameters);
}
