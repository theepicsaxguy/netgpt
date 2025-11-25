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
    [Route("admin/agent-threads")]
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

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAgentThread(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            GetAgentThreadByIdQuery query = new(id);
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelAgentThread(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            CancelAgentThreadCommand command = new(id);
            await mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpPost("{id}/resume")]
        public async Task<IActionResult> ResumeAgentThread(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            ResumeAgentThreadCommand command = new(id);
            await mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpPost("{id}/rerun")]
        public async Task<IActionResult> RerunAgentThread(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            RerunAgentThreadCommand command = new(id);
            var result = await mediator.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}
