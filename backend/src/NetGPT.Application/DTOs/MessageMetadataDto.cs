// <copyright file="MessageMetadataDto.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record MessageMetadataDto(
        List<ToolInvocationDto>? ToolInvocations = null,
        string? AgentName = null,
        int? TokenCount = null);
}
