// Copyright (c) 2025 NetGPT. All rights reserved.

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

namespace NetGPT.API.Hubs
{
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
                Conversation? conversation = await repository.GetByIdAsync(ConversationId.From(conversationId));
                if (conversation == null || conversation.UserId != UserId.From(userId))
                {
                    await Clients.Caller.SendAsync("Error", "Conversation not found or unauthorized");
                    return;
                }

                // Add user message
                Guid messageId = Guid.NewGuid();
                await Clients.Caller.SendAsync("MessageStarted", messageId);

                // Stream agent response
                await foreach (StreamingChunkDto chunk in orchestrator.ExecuteStreamingAsync(conversation, content))
                {
                    await Clients.Caller.SendAsync("MessageChunk", chunk);
                }

                await Clients.Caller.SendAsync("MessageCompleted", messageId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        private static Guid GetCurrentUserId()
        {
            // TODO: Get from JWT claims
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
    }
}
