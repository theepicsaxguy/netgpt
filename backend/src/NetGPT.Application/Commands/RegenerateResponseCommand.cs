// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Commands
{
    public sealed record RegenerateResponseCommand(
        Guid ConversationId,
        Guid UserId,
        Guid MessageId) : IRequest<Result<MessageResponse>>;
}
