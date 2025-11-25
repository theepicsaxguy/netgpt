// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.Commands.Admin;
using NetGPT.Application.DTOs;

namespace NetGPT.Application.Handlers.Admin
{
    public sealed class RerunAgentThreadHandler : IRequestHandler<RerunAgentThreadCommand, AgentThreadSummaryDto>
    {
        public Task<AgentThreadSummaryDto> Handle(RerunAgentThreadCommand request, CancellationToken cancellationToken)
        {
            // TODO: implement rerun logic
            return Task.FromResult(new AgentThreadSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Running", DateTime.UtcNow, null)); // placeholder
        }
    }
}
