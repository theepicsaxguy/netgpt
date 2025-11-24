// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Queries
{
    public sealed record SearchConversationsQuery(
        Guid UserId,
        string SearchTerm,
        int Page,
        int PageSize) : IRequest<Result<PaginatedResponse<ConversationResponse>>>;
}
