// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Entities;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Interfaces
{
    public interface IAgentOrchestrator
    {
        Task<Result<AgentResponse>> ExecuteAsync(
            Conversation conversation,
            string userMessage,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<StreamingChunkDto> ExecuteStreamingAsync(
            Conversation conversation,
            string userMessage,
            CancellationToken cancellationToken = default);

        // Execute an IAgentExecutable produced by the DeclarativeLoader and
        // return a standard AgentResponse result. This is used for executing
        // persisted declarative definitions.
        Task<Result<AgentResponse>> ExecuteDefinitionAsync(
            DefinitionEntity definition,
            IAgentExecutable executable,
            string input,
            CancellationToken cancellationToken = default);
    }
}
