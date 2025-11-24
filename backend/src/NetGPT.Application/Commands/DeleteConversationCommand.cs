// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using MediatR;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Commands
{
    public sealed record DeleteConversationCommand(
        Guid ConversationId,
        Guid UserId) : IRequest<Result>;
}
