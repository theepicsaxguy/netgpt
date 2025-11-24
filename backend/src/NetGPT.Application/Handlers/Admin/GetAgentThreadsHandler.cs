using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Queries.Admin;
using System;
using System.Collections.Generic;

namespace NetGPT.Application.Handlers.Admin;

public sealed class GetAgentThreadsHandler : IRequestHandler<GetAgentThreadsQuery, PaginatedAgentThreadListDto>
{
    public Task<PaginatedAgentThreadListDto> Handle(GetAgentThreadsQuery request, CancellationToken cancellationToken)
    {
        // TODO: implement data access and mapping from domain
        var items = new List<AgentThreadSummaryDto>();
        var dto = new PaginatedAgentThreadListDto(items, 0, request.Page, request.PageSize);
        return Task.FromResult(dto);
    }
}
