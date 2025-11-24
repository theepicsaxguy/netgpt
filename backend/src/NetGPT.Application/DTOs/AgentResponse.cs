// <copyright file="AgentResponse.cs" theepicsaxguy">
// \
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
