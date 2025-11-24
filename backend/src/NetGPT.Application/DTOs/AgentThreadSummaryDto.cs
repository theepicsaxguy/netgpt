// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    public record AgentThreadSummaryDto(
        Guid Id,
        Guid ConversationId,
        string Status,
        DateTime StartedAt,
        DateTime? EndedAt);
}
