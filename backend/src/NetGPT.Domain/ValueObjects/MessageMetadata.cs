// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;

namespace NetGPT.Domain.ValueObjects
{
    public record MessageMetadata(
        List<ToolInvocation>? ToolInvocations = null,
        string? AgentName = null,
        int? TokenCount = null,
        Dictionary<string, object>? CustomProperties = null);
}
