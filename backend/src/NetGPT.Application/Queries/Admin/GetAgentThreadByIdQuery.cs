// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using MediatR;
using NetGPT.Application.DTOs;

namespace NetGPT.Application.Queries.Admin
{
    public sealed record GetAgentThreadByIdQuery(Guid ThreadId) : IRequest<AgentThreadDetailDto>;
}
