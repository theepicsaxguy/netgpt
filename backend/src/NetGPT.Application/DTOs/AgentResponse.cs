// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Application.DTOs
{
    public sealed record AgentResponse(
        string Content,
        int TokensUsed,
        TimeSpan ResponseTime,
        string? ModelUsed = null);
}
