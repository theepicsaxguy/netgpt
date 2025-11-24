// <copyright file="ConversationQueries.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Queries
{
    using System;
    using System.Collections.Generic;
    using MediatR;
    using NetGPT.Application.DTOs;
    using NetGPT.Domain.Primitives;

    public sealed record GetConversationQuery(
        Guid ConversationId,
        Guid UserId) : IRequest<Result<ConversationResponse>>;

    public sealed record GetConversationsQuery(
        Guid UserId,
        int Page,
        int PageSize) : IRequest<Result<PaginatedResponse<ConversationResponse>>>;

    public sealed record GetMessagesQuery(
        Guid ConversationId,
        Guid UserId) : IRequest<Result<List<MessageResponse>>>;

    public sealed record SearchConversationsQuery(
        Guid UserId,
        string SearchTerm,
        int Page,
        int PageSize) : IRequest<Result<PaginatedResponse<ConversationResponse>>>;
}
