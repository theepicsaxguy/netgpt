// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Queries
{
    public record ListConversationsQuery(
        UserId UserId,
        int Page = 1,
        int PageSize = 50) : IRequest<PaginatedResult<ConversationDto>>;
}
