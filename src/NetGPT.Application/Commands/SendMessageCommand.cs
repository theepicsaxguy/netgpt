using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Commands;

public record SendMessageCommand(
    ConversationId ConversationId,
    UserId UserId,
    string Content,
    List<Guid>? AttachmentIds = null) : IRequest<MessageDto>;