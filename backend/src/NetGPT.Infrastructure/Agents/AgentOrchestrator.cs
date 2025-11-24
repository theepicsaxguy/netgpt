// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Primitives;
using NetGPT.Domain.ValueObjects;
using NetGPT.Infrastructure.Tools;

namespace NetGPT.Infrastructure.Agents
{
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
                DateTime startTime = DateTime.UtcNow;
                string responseText = string.Empty;
                int tokenCount = 0;

                if (conversation.AgentConfiguration.IsMultiAgent)
                {
                    // Multi-agent orchestration using group chat
                    var agents = new List<AIAgent>();
                    foreach (var agentDef in conversation.AgentConfiguration.Agents!)
                    {
                        var agent = await agentFactory.CreateAgentAsync(agentDef, toolRegistry.GetAllTools());
                        agents.Add(agent);
                    }

                    var workflow = AgentWorkflowBuilder
                        .CreateGroupChatBuilderWith(agents => new RoundRobinGroupChatManager(agents) { MaximumIterationCount = 5 })
                        .AddParticipants(agents.ToArray())
                        .Build();

                    var result = await workflow.RunAsync(new[] { new ChatMessage(ChatRole.User, userMessage) }, cancellationToken: cancellationToken);
                    responseText = result.LastOrDefault()?.Text ?? string.Empty;
                }
                else
                {
                    // Single-agent execution
                    AIAgent agent = await agentFactory.CreatePrimaryAgentAsync(
                        conversation.AgentConfiguration,
                        toolRegistry.GetAllTools());

                    // Run agent with user message
                    AgentRunResponse result = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);

                    // Extract the text response from AgentRunResponse
                    responseText = result.Messages.LastOrDefault()?.Text ?? string.Empty;
                }

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
