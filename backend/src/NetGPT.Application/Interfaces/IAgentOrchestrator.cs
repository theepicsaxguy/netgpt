// <copyright file="IAgentOrchestrator.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using NetGPT.Application.DTOs;
    using NetGPT.Domain.Aggregates;
    using NetGPT.Domain.Primitives;

    public interface IAgentOrchestrator
    {
        Task<Result<AgentResponse>> ExecuteAsync(
            Conversation conversation,
            string userMessage,
            CancellationToken cancellationToken = default);
    }
}
