// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using MediatR;
using NetGPT.Application.DTOs;

namespace NetGPT.Application.Commands.Admin
{
    public sealed record RerunAgentThreadCommand(Guid ThreadId) : IRequest<AgentThreadSummaryDto>;
}
