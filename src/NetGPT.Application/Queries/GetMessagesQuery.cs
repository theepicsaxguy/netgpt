using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Queries;

public record GetMessagesQuery(
    ConversationId ConversationId,
    UserId UserId) : IRequest<List<MessageDto>>;