// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.API.Hubs
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public sealed class ConversationHub(
        IConversationRepository repository,
        IAgentOrchestrator orchestrator) : Hub
    {
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

        private Guid GetCurrentUserId()
        {
            ClaimsPrincipal? user = Context.User ?? throw new InvalidOperationException("User context is not available");
            HttpContext? httpContext = Context.GetHttpContext();
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                string? sub = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.User.FindFirst("sub")?.Value
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(sub, out Guid userId))
                {
                    return userId;
                }
            }

            throw new InvalidOperationException("Unable to determine current user from claims");
        }
    }
}
