// <copyright file="GetAgentThreadByIdQuery.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.Queries.Admin
{
    using System;
    using MediatR;
    using NetGPT.Application.DTOs;

    public sealed record GetAgentThreadByIdQuery(Guid ThreadId) : IRequest<AgentThreadDetailDto>;
}
