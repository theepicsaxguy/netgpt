// <copyright file="ListConversationsQuery.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Queries
{
    using System;
    using System.Collections.Generic;
    using MediatR;
    using NetGPT.Application.DTOs;
    using NetGPT.Domain.ValueObjects;

    public record ListConversationsQuery(
        UserId UserId,
        int Page = 1,
        int PageSize = 50) : IRequest<PaginatedResult<ConversationDto>>;

    public record PaginatedResult<T>(
        List<T> Items,
        int TotalCount,
        int Page,
        int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling(this.TotalCount / (double)this.PageSize);

        public bool HasNextPage => this.Page < this.TotalPages;
    }
}
