// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.Queries.Admin
{
    using System;
    using MediatR;
    using NetGPT.Application.DTOs;

    public sealed record GetAgentThreadByIdQuery(Guid ThreadId) : IRequest<AgentThreadDetailDto>;
}
