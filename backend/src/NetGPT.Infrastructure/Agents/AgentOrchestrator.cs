using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Enums;
using NetGPT.Domain.Primitives;
using NetGPT.Infrastructure.Tools;
using OpenAI;

namespace NetGPT.Infrastructure.Agents;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IAgentFactory _agentFactory;
    private readonly IToolRegistry _toolRegistry;

    public AgentOrchestrator(IAgentFactory agentFactory, IToolRegistry toolRegistry)
    {
        _agentFactory = agentFactory;
        _toolRegistry = toolRegistry;
    }

    public async Task<Result<AgentResponse>> ExecuteAsync(
        Conversation conversation,
        string userMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var agent = await _agentFactory.CreatePrimaryAgentAsync(
                conversation.AgentConfiguration,
                _toolRegistry.GetAllTools());

            var startTime = DateTime.UtcNow;
            var responseText = string.Empty;
            var tokenCount = 0;

            // Run agent with user message
            var result = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);
            // Extract the text response from AgentRunResponse
            responseText = result.Messages.LastOrDefault()?.Text ?? string.Empty;

            var responseTime = DateTime.UtcNow - startTime;
            tokenCount = EstimateTokens(userMessage + responseText);

            var response = new AgentResponse(
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

    private static List<ChatMessage> BuildChatHistory(Conversation conversation)
    {
        return conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessage(
                m.Role == MessageRole.User ? ChatRole.User : ChatRole.Assistant,
                m.Content.Text))
            .ToList();
    }

    private static int EstimateTokens(string text) => (int)(text.Length / 4.0);
}
