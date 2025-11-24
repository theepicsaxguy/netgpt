// <copyright file="MessageMetadataDto.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record MessageMetadataDto(
        List<ToolInvocationDto>? ToolInvocations = null,
        string? AgentName = null,
        int? TokenCount = null);
}
