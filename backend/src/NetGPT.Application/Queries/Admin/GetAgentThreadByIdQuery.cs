using System;
using MediatR;
using NetGPT.Application.DTOs;

namespace NetGPT.Application.Queries.Admin;

public sealed record GetAgentThreadByIdQuery(Guid ThreadId) : IRequest<AgentThreadDetailDto>;
