using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Enums;
using NetGPT.Domain.Exceptions;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Handlers;

public class SendMessageHandler : IRequestHandler<SendMessageCommand, Result<MessageResponse>>
{
    private readonly IConversationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageHandler(
        IConversationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MessageResponse>> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var conversationId = ConversationId.From(request.ConversationId);
        var userId = UserId.From(request.UserId);

        var conversation = await _repository.GetByIdAsync(conversationId, ct);
        if (conversation is null)
        {
            return Result.Failure<MessageResponse>(new Error("Conversation.NotFound", "Conversation not found"));
        }

        conversation.EnsureOwnership(userId);

        var content = MessageContent.FromText(request.Content);
        var messageId = conversation.AddMessage(MessageRole.User, content);

        await _unitOfWork.SaveChangesAsync(ct);

        var message = conversation.GetMessage(messageId);

        var response = new MessageResponse(
            message.Id.Value,
            message.Role.ToString(),
            message.Content.Text,
            message.CreatedAt
        );

        return Result.Success(response);
    }
}