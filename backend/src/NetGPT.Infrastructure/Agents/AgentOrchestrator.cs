using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
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

            var messages = BuildChatHistory(conversation);
            messages.Add(new ChatMessage(ChatRole.User, userMessage));

            var startTime = DateTime.UtcNow;
            var responseText = string.Empty;
            var toolsInvoked = new List<string>();
            var tokenCount = 0;

            await foreach (var update in agent.RunStreamAsync(messages, cancellationToken))
            {
                if (update.Text != null)
                {
                    responseText += update.Text;
                }

                if (update.ToolCalls != null)
                {
                    foreach (var toolCall in update.ToolCalls)
                    {
                        toolsInvoked.Add(toolCall.FunctionName);
                    }
                }
            }

            var responseTime = DateTime.UtcNow - startTime;
            tokenCount = EstimateTokens(userMessage + responseText);

            return new AgentResponse(
                responseText,
                tokenCount,
                responseTime,
                toolsInvoked);
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
                m.Content.Role == MessageRole.User ? ChatRole.User : ChatRole.Assistant,
                m.Content.Text))
            .ToList();
    }

    private static int EstimateTokens(string text) => (int)(text.Length / 4.0);
}

public sealed record AgentResponse(
    string Text,
    int TokensUsed,
    TimeSpan ResponseTime,
    List<string> ToolsInvoked);
