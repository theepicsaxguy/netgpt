// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Entities;
using NetGPT.Domain.Primitives;

namespace NetGPT.Infrastructure.Agents
{
    /// <summary>
    /// Partial class for declarative definition execution.
    /// </summary>
    public sealed partial class AgentOrchestrator
    {
        public async Task<Result<AgentResponse>> ExecuteDefinitionAsync(
            DefinitionEntity definition,
            IAgentExecutable executable,
            string input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNull(executable);

            DateTime start = DateTime.UtcNow;
            Guid execId = Guid.NewGuid();

            try
            {
                AgentRunResponse run = await executable.ExecuteAsync(input, cancellationToken);
                DateTime end = DateTime.UtcNow;

                int tokenCount = EstimateTokens(string.Join("\n", run.Messages.Select(m => m.Text ?? string.Empty)));

                AgentResponse agentResponse = new(
                    Content: run.Messages.LastOrDefault()?.Text ?? string.Empty,
                    TokensUsed: tokenCount,
                    ResponseTime: end - start,
                    ModelUsed: definition.Id.ToString());

                DeclarativeExecutionFinished(logger, definition.Id, definition.Version, execId, start, end, null);

                return Result.Success(agentResponse);
            }
            catch (Exception ex)
            {
                DateTime end = DateTime.UtcNow;
                DeclarativeExecutionFailed(logger, definition.Id, definition.Version, execId, ex);
                return Result.Failure<AgentResponse>(new DomainError("DeclarativeExecutionFailed", ex.Message));
            }
        }
    }
}
