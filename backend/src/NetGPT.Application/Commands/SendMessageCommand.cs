// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Commands
{
    public sealed record SendMessageCommand(
        Guid ConversationId,
        Guid UserId,
        string Content,
        List<FileAttachmentDto>? Attachments,
        bool AllowTools = true,
        RunOptionsDto? RunOptions = null) : IRequest<Result<MessageResponse>>;
}
