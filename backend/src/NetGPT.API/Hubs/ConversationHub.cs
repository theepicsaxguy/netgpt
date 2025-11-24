// <copyright file="ConversationHub.cs" company="PlaceholderCompany">
// \
// </copyright>

namespace NetGPT.API.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.SignalR;
    using NetGPT.Application.DTOs;
    using NetGPT.Application.Interfaces;
    using NetGPT.Domain.Aggregates;
    using NetGPT.Domain.Interfaces;
    using NetGPT.Domain.ValueObjects;

    public sealed class ConversationHub(
        IMediator mediator,
        IConversationRepository repository,
        IAgentOrchestrator orchestrator) : Hub
    {
        private readonly IMediator mediator = mediator;
        private readonly IConversationRepository repository = repository;
        private readonly IAgentOrchestrator orchestrator = orchestrator;

        public async Task SendMessage(Guid conversationId, string content)
        {
            Guid userId = GetCurrentUserId();

            try
            {
                Conversation? conversation = await this.repository.GetByIdAsync(ConversationId.From(conversationId));
                if (conversation == null || conversation.UserId != UserId.From(userId))
                {
                    await this.Clients.Caller.SendAsync("Error", "Conversation not found or unauthorized");
                    return;
                }

                // Add user message
                Guid messageId = Guid.NewGuid();
                await this.Clients.Caller.SendAsync("MessageStarted", messageId);

                // Stream agent response
                await foreach (StreamingChunkDto chunk in StreamAgentResponse(conversation, content))
                {
                    await this.Clients.Caller.SendAsync("MessageChunk", chunk);
                }

                await this.Clients.Caller.SendAsync("MessageCompleted", messageId);
            }
            catch (Exception ex)
            {
                await this.Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        private static async IAsyncEnumerable<StreamingChunkDto> StreamAgentResponse(
            Conversation conversation,
            string userMessage)
        {
            // Generate a new messageId for the streaming response
            Guid messageId = Guid.NewGuid();

            // This would integrate with actual Agent Framework streaming
            yield return new StreamingChunkDto(messageId, "Hello", null, false);
            await Task.Delay(100);
            yield return new StreamingChunkDto(messageId, " World", null, true);
        }

        private static Guid GetCurrentUserId()
        {
            // TODO: Get from JWT claims
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
    }
}
