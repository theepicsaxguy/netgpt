using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Commands;

public record CreateConversationCommand(
    UserId UserId,
    string? Title = null) : IRequest<ConversationDto>;