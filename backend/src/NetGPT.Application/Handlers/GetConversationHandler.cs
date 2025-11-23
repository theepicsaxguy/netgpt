using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Queries;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Handlers;

public sealed class GetConversationHandler : IRequestHandler<GetConversationQuery, Result<ConversationResponse>>
{
    private readonly IConversationRepository _repository;
    private readonly IConversationMapper _mapper;

    public GetConversationHandler(IConversationRepository repository, IConversationMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ConversationResponse>> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var conversationId = ConversationId.From(request.ConversationId);
        var userId = UserId.From(request.UserId);

        var conversation = await _repository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure<ConversationResponse>(
                new Error("Conversation.NotFound", "Conversation not found"));
        }

        if (conversation.UserId != userId)
        {
            return Result.Failure<ConversationResponse>(
                new Error("Conversation.Unauthorized", "Unauthorized access"));
        }

        return _mapper.ToResponse(conversation);
    }
}
