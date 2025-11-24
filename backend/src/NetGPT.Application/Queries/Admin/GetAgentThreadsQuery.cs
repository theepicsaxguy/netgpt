// Copyright (c) 2025 NetGPT. All rights reserved.

using MediatR;
using NetGPT.Application.DTOs;

namespace NetGPT.Application.Queries.Admin
{
    public sealed record GetAgentThreadsQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedAgentThreadListDto>;
}
