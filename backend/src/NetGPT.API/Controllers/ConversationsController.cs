// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetGPT.Application.Commands;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Queries;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;

namespace NetGPT.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class ConversationsController(IMediator mediator, IAgentOrchestrator orchestrator, IConversationRepository repository) : ControllerBase
    {
        private readonly IMediator mediator = mediator;
        private readonly IAgentOrchestrator orchestrator = orchestrator;
        private readonly IConversationRepository repository = repository;

        [HttpPost]
        public async Task<IActionResult> CreateConversation(
            [FromBody] CreateConversationRequest request,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();
            CreateConversationCommand command = new(userId, request.Title, request.Configuration);
            Result<ConversationResponse> result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new { error = result.Error.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConversation(
            Guid id,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();
            GetConversationQuery query = new(id, userId);
            Result<ConversationResponse> result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(new { error = result.Error.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            Guid userId = GetCurrentUserId();
            GetConversationsQuery query = new(userId, page, pageSize);
            Result<PaginatedResponse<ConversationResponse>> result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new { error = result.Error.Message });
        }

        [HttpPost("{id}/messages")]
        public async Task<IActionResult> SendMessage(
            Guid id,
            [FromBody] SendMessageRequest request,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();
            SendMessageCommand command = new(id, userId, request.Content, request.Attachments);
            Result<MessageResponse> result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new { error = result.Error.Message });
        }

        [HttpPost("{id}/messages/stream")]
        public async Task StreamMessage(
            Guid id,
            [FromBody] SendMessageRequest request,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();

            // Retrieve the domain conversation to ensure permissions and configuration
            Conversation? conversation = await repository.GetByIdAsync(Domain.ValueObjects.ConversationId.From(id), cancellationToken);
            if (conversation == null || conversation.UserId != Domain.ValueObjects.UserId.From(userId))
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                await Response.WriteAsync(JsonSerializer.Serialize(new { error = "Conversation not found or unauthorized" }), cancellationToken);
                return;
            }

            // Set NDJSON response headers
            Response.ContentType = "application/x-ndjson";

            // Execute orchestrator streaming and write each chunk as a JSON line
            await foreach (StreamingChunkDto chunk in orchestrator.ExecuteStreamingAsync(
                conversation,
                request.Content,
                cancellationToken))
            {
                string json = JsonSerializer.Serialize(chunk);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConversation(
            Guid id,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();
            DeleteConversationCommand command = new(id, userId);
            Result result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : BadRequest(new { error = result.Error.Message });
        }

        private static Guid GetCurrentUserId()
        {
            // TODO: Get from JWT claims
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
    }
}
