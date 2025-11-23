using System.Collections.Generic;

namespace NetGPT.Application.DTOs;

public record AgentConfigurationDto(
    string? ModelName = null,
    float? Temperature = null,
    int? MaxTokens = null,
    Dictionary<string, object>? CustomProperties = null);
