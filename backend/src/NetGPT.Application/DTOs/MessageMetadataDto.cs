// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;

namespace NetGPT.Application.DTOs
{
    public record MessageMetadataDto(
        List<ToolInvocationDto>? ToolInvocations = null,
        string? AgentName = null,
        int? TokenCount = null);
}
