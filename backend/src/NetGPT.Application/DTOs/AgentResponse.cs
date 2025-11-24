// <copyright file="AgentResponse.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System;

    public sealed record AgentResponse(
        string Content,
        int TokensUsed,
        TimeSpan ResponseTime,
        string? ModelUsed = null);
}
