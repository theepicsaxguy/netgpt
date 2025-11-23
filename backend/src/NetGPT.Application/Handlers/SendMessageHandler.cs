using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Enums;
using NetGPT.Domain.Exceptions;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Handlers;

public class SendMessageHandler : IRequestHandler<SendMessageCommand, MessageDto>
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

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId, ct)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

        conversation.EnsureOwnership(request.UserId);

        var content = MessageContent.FromText(request.Content);
        var messageId = conversation.AddMessage(MessageRole.User, content);

        await _unitOfWork.SaveChangesAsync(ct);

        var message = conversation.GetMessage(messageId);
        
        return new MessageDto(
            message.Id.Value,
            message.Role,
            message.Content.Text,
            message.CreatedAt
        );
    }
}