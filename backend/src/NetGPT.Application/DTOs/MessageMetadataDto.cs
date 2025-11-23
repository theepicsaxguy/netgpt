using System.Collections.Generic;

namespace NetGPT.Application.DTOs;

public record MessageMetadataDto(
    List<ToolInvocationDto>? ToolInvocations = null,
    string? AgentName = null,
    int? TokenCount = null);