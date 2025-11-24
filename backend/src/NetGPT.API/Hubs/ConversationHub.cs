using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.API.Hubs;

public sealed class ConversationHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IConversationRepository _repository;
    private readonly IAgentOrchestrator _orchestrator;

    public ConversationHub(
        IMediator mediator,
        IConversationRepository repository,
        IAgentOrchestrator orchestrator)
    {
        _mediator = mediator;
        _repository = repository;
        _orchestrator = orchestrator;
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var userId = GetCurrentUserId();

        try
        {
            var conversation = await _repository.GetByIdAsync(ConversationId.From(conversationId));
            if (conversation == null || conversation.UserId != UserId.From(userId))
            {
                await Clients.Caller.SendAsync("Error", "Conversation not found or unauthorized");
                return;
            }

            // Add user message
            var messageId = Guid.NewGuid();
            await Clients.Caller.SendAsync("MessageStarted", messageId);

            // Stream agent response
            await foreach (var chunk in StreamAgentResponse(conversation, content))
            {
                await Clients.Caller.SendAsync("MessageChunk", new
                {
                    MessageId = messageId,
                    Content = chunk.Content,
                    ToolInvocations = chunk.ToolInvocations,
                    IsComplete = chunk.IsComplete
                });
            }

            await Clients.Caller.SendAsync("MessageCompleted", messageId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    private async IAsyncEnumerable<StreamChunk> StreamAgentResponse(
        Domain.Aggregates.Conversation conversation,
        string userMessage)
    {
        // This would integrate with actual Agent Framework streaming
        yield return new StreamChunk("Hello", new List<string>(), false);
        await Task.Delay(100);
        yield return new StreamChunk(" World", new List<string>(), true);
    }

    private Guid GetCurrentUserId()
    {
        // TODO: Get from JWT claims
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}

public record StreamChunk(string Content, List<string> ToolInvocations, bool IsComplete);
