using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Interfaces;

namespace NetGPT.Application.Handlers;

public class CreateConversationHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IConversationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateConversationHandler(
        IConversationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConversationDto> Handle(
        CreateConversationCommand request,
        CancellationToken ct)
    {
        var conversation = Conversation.Create(request.UserId, request.Title);
        
        await _repository.AddAsync(conversation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new ConversationDto(
            conversation.Id.Value,
            conversation.Title,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            0
        );
    }
}