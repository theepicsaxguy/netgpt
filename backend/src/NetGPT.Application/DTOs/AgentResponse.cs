using System;

namespace NetGPT.Application.DTOs;

public sealed record AgentResponse(
    string Content,
    int TokensUsed,
    TimeSpan ResponseTime,
    string? ModelUsed = null);
