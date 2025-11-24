// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using MediatR;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Queries
{
    public sealed record GetMessagesQuery(
        Guid ConversationId,
        Guid UserId) : IRequest<Result<List<MessageResponse>>>;
}