// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Agents
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Agents.AI;
    using NetGPT.Application.DTOs;
    using NetGPT.Application.Interfaces;
    using NetGPT.Domain.Aggregates;
    using NetGPT.Domain.Primitives;
    using NetGPT.Infrastructure.Tools;

    public sealed class AgentOrchestrator(IAgentFactory agentFactory, IToolRegistry toolRegistry) : IAgentOrchestrator
    {
        private readonly IAgentFactory agentFactory = agentFactory;
        private readonly IToolRegistry toolRegistry = toolRegistry;

        public async Task<Result<AgentResponse>> ExecuteAsync(
            Conversation conversation,
            string userMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                AIAgent agent = await this.agentFactory.CreatePrimaryAgentAsync(
                    conversation.AgentConfiguration,
                    this.toolRegistry.GetAllTools());

                DateTime startTime = DateTime.UtcNow;
                string responseText = string.Empty;
                int tokenCount = 0;

                // Run agent with user message
                AgentRunResponse result = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);

                // Extract the text response from AgentRunResponse
                responseText = result.Messages.LastOrDefault()?.Text ?? string.Empty;

                TimeSpan responseTime = DateTime.UtcNow - startTime;
                tokenCount = EstimateTokens(userMessage + responseText);

                AgentResponse response = new(
                    Content: responseText,
                    TokensUsed: tokenCount,
                    ResponseTime: responseTime,
                    ModelUsed: conversation.AgentConfiguration.ModelName);

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<AgentResponse>(
                    new Error("AgentOrchestrator.ExecutionFailed", ex.Message));
            }
        }

        private static int EstimateTokens(string text)
        {
            return (int)(text.Length / 4.0);
        }
    }
}
