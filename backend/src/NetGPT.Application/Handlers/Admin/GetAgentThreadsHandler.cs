// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Queries.Admin;

namespace NetGPT.Application.Handlers.Admin
{
    public sealed class GetAgentThreadsHandler : IRequestHandler<GetAgentThreadsQuery, PaginatedAgentThreadListDto>
    {
        public Task<PaginatedAgentThreadListDto> Handle(GetAgentThreadsQuery request, CancellationToken cancellationToken)
        {
            // TODO: implement data access and mapping from domain
            List<AgentThreadSummaryDto> items = [];
            PaginatedAgentThreadListDto dto = new(items, 0, request.Page, request.PageSize);
            return Task.FromResult(dto);
        }
    }
}
