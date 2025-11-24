// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using MediatR;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Commands
{
    public sealed record UpdateConversationCommand(
        Guid ConversationId,
        Guid UserId,
        string Title) : IRequest<Result<ConversationResponse>>;
}