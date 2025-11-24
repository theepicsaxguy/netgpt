// <copyright file="ConversationsController.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.API.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using NetGPT.Application.Commands;
    using NetGPT.Application.DTOs;
    using NetGPT.Application.Queries;
    using NetGPT.Domain.Primitives;

    [ApiController]
    [Route("api/v1/[controller]")]
    public sealed class ConversationsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> CreateConversation(
            [FromBody] CreateConversationRequest request,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();
            CreateConversationCommand command = new(userId, request.Title, request.Configuration);
            Result<ConversationResponse> result = await this.mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? this.Ok(result.Value)
                : this.BadRequest(new { error = result.Error.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConversation(
            Guid id,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();
            GetConversationQuery query = new(id, userId);
            Result<ConversationResponse> result = await this.mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? this.Ok(result.Value)
                : this.NotFound(new { error = result.Error.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            Guid userId = GetCurrentUserId();
            GetConversationsQuery query = new(userId, page, pageSize);
            Result<PaginatedResponse<ConversationResponse>> result = await this.mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? this.Ok(result.Value)
                : this.BadRequest(new { error = result.Error.Message });
        }

        [HttpPost("{id}/messages")]
        public async Task<IActionResult> SendMessage(
            Guid id,
            [FromBody] SendMessageRequest request,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();
            SendMessageCommand command = new(id, userId, request.Content, request.Attachments);
            Result<MessageResponse> result = await this.mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? this.Ok(result.Value)
                : this.BadRequest(new { error = result.Error.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConversation(
            Guid id,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();
            DeleteConversationCommand command = new(id, userId);
            Result result = await this.mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? this.NoContent()
                : this.BadRequest(new { error = result.Error.Message });
        }

        private static Guid GetCurrentUserId()
        {
            // TODO: Get from JWT claims
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
    }
}
