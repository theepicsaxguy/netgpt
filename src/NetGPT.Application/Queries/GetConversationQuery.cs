using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Queries;

public record GetConversationQuery(
    ConversationId ConversationId,
    UserId UserId) : IRequest<ConversationDto?>;