using System;
using System.Collections.Generic;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Commands;

public sealed record CreateConversationCommand(
    Guid UserId,
    string? Title,
    AgentConfigurationDto? Configuration) : IRequest<Result<ConversationResponse>>;

public sealed record SendMessageCommand(
    Guid ConversationId,
    Guid UserId,
    string Content,
    List<FileAttachmentDto>? Attachments) : IRequest<Result<MessageResponse>>;

public sealed record SendMessageStreamingCommand(
    Guid ConversationId,
    Guid UserId,
    string Content,
    List<FileAttachmentDto>? Attachments) : IRequest<Result<Guid>>;

public sealed record UpdateConversationCommand(
    Guid ConversationId,
    Guid UserId,
    string Title) : IRequest<Result<ConversationResponse>>;

public sealed record DeleteConversationCommand(
    Guid ConversationId,
    Guid UserId) : IRequest<Result>;

public sealed record RegenerateResponseCommand(
    Guid ConversationId,
    Guid UserId,
    Guid MessageId) : IRequest<Result<MessageResponse>>;
