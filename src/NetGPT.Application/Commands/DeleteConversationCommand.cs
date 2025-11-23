using MediatR;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Commands;

public record DeleteConversationCommand(
    ConversationId ConversationId,
    UserId UserId) : IRequest;