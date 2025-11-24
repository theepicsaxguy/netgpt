// <copyright file="AgentConfigurationDto.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record AgentConfigurationDto(
        string? ModelName = null,
        float? Temperature = null,
        int? MaxTokens = null,
        Dictionary<string, object>? CustomProperties = null);
}
