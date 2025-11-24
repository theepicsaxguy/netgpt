// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Interfaces
{
    public interface IAgentOrchestrator
    {
        Task<Result<AgentResponse>> ExecuteAsync(
            Conversation conversation,
            string userMessage,
            CancellationToken cancellationToken = default);
    }
}
