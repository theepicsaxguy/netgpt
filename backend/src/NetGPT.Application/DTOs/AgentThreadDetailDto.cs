// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;

namespace NetGPT.Application.DTOs
{
    public record AgentThreadDetailDto(
        Guid Id,
        Guid ConversationId,
        string Status,
        DateTime StartedAt,
        DateTime? EndedAt,
        IEnumerable<ToolInvocationDetailDto> ToolInvocations,
        IEnumerable<StreamingChunkDetailDto> StreamingChunks);
}
