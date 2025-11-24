// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.Queries.Admin
{
    using MediatR;
    using NetGPT.Application.DTOs;

    public sealed record GetAgentThreadsQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedAgentThreadListDto>;
}
