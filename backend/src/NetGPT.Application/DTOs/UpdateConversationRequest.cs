// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    public record UpdateConversationRequest(
        string? Title = null,
        AgentConfigurationDto? Configuration = null);
}
