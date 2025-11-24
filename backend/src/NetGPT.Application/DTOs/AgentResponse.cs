// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    using System;

    public sealed record AgentResponse(
        string Content,
        int TokensUsed,
        TimeSpan ResponseTime,
        string? ModelUsed = null);
}
