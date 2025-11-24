// <copyright file="GetAgentThreadsQuery.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Queries.Admin
{
    using MediatR;
    using NetGPT.Application.DTOs;

    public sealed record GetAgentThreadsQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedAgentThreadListDto>;
}
