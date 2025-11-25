// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetGPT.Application.Commands.Admin;
using NetGPT.Application.Queries.Admin;
using NetGPT.Domain.Primitives;

namespace NetGPT.API.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/agent-threads")]
    [Authorize(Policy = "AdminOnly")]
    public sealed class AgentThreadsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAgentThreads(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            GetAgentThreadsQuery query = new(page, pageSize);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new { error = result.Error.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAgentThread(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            GetAgentThreadQuery query = new(id);
            var result = await mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return result.Error is not null && result.Error.Code == "NotFound"
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error?.Message });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelAgentThread(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            CancelAgentThreadCommand command = new(id);
            Result result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            return result.Error is not null && result.Error.Code == "NotFound"
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error?.Message });
        }

        [HttpPost("{id}/resume")]
        public async Task<IActionResult> ResumeAgentThread(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            ResumeAgentThreadCommand command = new(id);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return result.Error is not null && result.Error.Code == "NotFound"
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error?.Message });
        }

        [HttpPost("{id}/rerun")]
        public async Task<IActionResult> RerunAgentThread(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            RerunAgentThreadCommand command = new(id);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return result.Error is not null && result.Error.Code == "NotFound"
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error?.Message });
        }
    }
}
