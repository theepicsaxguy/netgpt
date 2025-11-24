// <copyright file="CreateConversationRequest.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    public record CreateConversationRequest(
        string? Title = null,
        AgentConfigurationDto? Configuration = null);
}
