using MediatR;
using NetGPT.Application.DTOs;
using System.Collections.Generic;

namespace NetGPT.Application.Queries.Admin;

public sealed record GetAgentThreadsQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedAgentThreadListDto>;
