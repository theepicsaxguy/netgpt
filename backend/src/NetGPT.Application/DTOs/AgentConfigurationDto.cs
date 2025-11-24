// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;

namespace NetGPT.Application.DTOs
{
    public record AgentDefinitionDto(
        string Name,
        string Instructions,
        string? ModelName = null,
        float? Temperature = null,
        int? MaxTokens = null);

    public record AgentConfigurationDto(
        string? ModelName = null,
        float? Temperature = null,
        int? MaxTokens = null,
        Dictionary<string, object>? CustomProperties = null,
        IReadOnlyList<AgentDefinitionDto>? Agents = null);
}
