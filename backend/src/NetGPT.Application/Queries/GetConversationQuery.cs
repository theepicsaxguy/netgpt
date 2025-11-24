// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using MediatR;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Queries
{
    public sealed record GetConversationQuery(
        Guid ConversationId,
        Guid UserId) : IRequest<Result<ConversationResponse>>;
}