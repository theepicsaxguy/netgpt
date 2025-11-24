using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using NetGPT.Application.DTOs;
using NetGPT.Domain.ValueObjects;
using NetGPT.Infrastructure.Tools;
using OpenAI;

namespace NetGPT.Infrastructure.Agents.Workflows;

public class ConversationWorkflow : IWorkflow
{
    private readonly IOpenAIClientFactory _clientFactory;
    private readonly ToolRegistry _toolRegistry;

    public ConversationWorkflow(
        IOpenAIClientFactory clientFactory,
        ToolRegistry toolRegistry)
    {
        _clientFactory = clientFactory;
        _toolRegistry = toolRegistry;
    }

    public async IAsyncEnumerable<StreamingChunkDto> ExecuteAsync(
        WorkflowContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var chatClient = _clientFactory.CreateChatClient();

        var agent = chatClient.CreateAIAgent(
            instructions: "You are a helpful AI assistant. Use tools when needed.");

        var messageId = MessageId.CreateNew();

        // Simple non-streaming response for now
        var result = await agent.RunAsync(context.UserMessage, cancellationToken: ct);
        var responseText = result.Messages.LastOrDefault()?.Text ?? string.Empty;

        yield return new StreamingChunkDto(
            MessageId: messageId.Value,
            Content: responseText,
            IsComplete: false
        );

        yield return new StreamingChunkDto(
            MessageId: messageId.Value,
            Content: null,
            IsComplete: true
        );
    }

    private static List<ChatMessage> BuildMessageHistory(WorkflowContext context)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant.")
        };

        foreach (var msg in context.Conversation.Messages.TakeLast(10))
        {
            var role = msg.Role switch
            {
                Domain.Enums.MessageRole.User => ChatRole.User,
                Domain.Enums.MessageRole.Assistant => ChatRole.Assistant,
                Domain.Enums.MessageRole.System => ChatRole.System,
                _ => ChatRole.User
            };

            messages.Add(new ChatMessage(role, msg.Content.Text));
        }

        messages.Add(new ChatMessage(ChatRole.User, context.UserMessage));
        return messages;
    }
}