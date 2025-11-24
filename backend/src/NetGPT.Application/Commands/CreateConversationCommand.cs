// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Commands
{
    public sealed record CreateConversationCommand(
        Guid UserId,
        string? Title,
        AgentConfigurationDto? Configuration) : IRequest<Result<ConversationResponse>>;
}
