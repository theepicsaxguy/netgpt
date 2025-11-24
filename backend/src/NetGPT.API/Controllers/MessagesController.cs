// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using NetGPT.Application.DTOs;
    using NetGPT.Application.Queries;
    using NetGPT.Domain.Primitives;

    [ApiController]
    [Route("api/v1/conversations/{conversationId}/messages")]
    public sealed class MessagesController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetMessages(
            Guid conversationId,
            CancellationToken cancellationToken)
        {
            Guid userId = GetCurrentUserId();
            GetMessagesQuery query = new(conversationId, userId);
            Result<List<MessageResponse>> result = await this.mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? this.Ok(result.Value)
                : this.NotFound(new { error = result.Error.Message });
        }

        private static Guid GetCurrentUserId()
        {
            // TODO: Get from JWT claims
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
    }
}
