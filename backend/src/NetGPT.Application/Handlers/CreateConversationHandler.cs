using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Handlers;

public class CreateConversationHandler : IRequestHandler<CreateConversationCommand, Result<ConversationResponse>>
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

    public async Task<Result<ConversationResponse>> Handle(
        CreateConversationCommand request,
        CancellationToken ct)
    {
        var userId = UserId.From(request.UserId);
        var conversation = Conversation.Create(userId, request.Title);

        await _repository.AddAsync(conversation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var response = new ConversationResponse(
            conversation.Id.Value,
            conversation.Title,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            0
        );

        return Result.Success(response);
    }
}