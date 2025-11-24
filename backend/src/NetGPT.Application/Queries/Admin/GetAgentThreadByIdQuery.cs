// <copyright file="GetAgentThreadByIdQuery.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Queries.Admin
{
    using System;
    using MediatR;
    using NetGPT.Application.DTOs;

    public sealed record GetAgentThreadByIdQuery(Guid ThreadId) : IRequest<AgentThreadDetailDto>;
}
