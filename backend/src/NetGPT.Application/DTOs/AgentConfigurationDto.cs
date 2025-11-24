// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record AgentConfigurationDto(
        string? ModelName = null,
        float? Temperature = null,
        int? MaxTokens = null,
        Dictionary<string, object>? CustomProperties = null);
}
