using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Queries;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Handlers;

public sealed class GetMessagesHandler : IRequestHandler<GetMessagesQuery, Result<List<MessageResponse>>>
{
    private readonly IConversationRepository _repository;
    private readonly IConversationMapper _mapper;

    public GetMessagesHandler(IConversationRepository repository, IConversationMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<MessageResponse>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure<List<MessageResponse>>(
                new Error("Conversation.NotFound", "Conversation not found"));
        }

        if (conversation.UserId != request.UserId)
        {
            return Result.Failure<List<MessageResponse>>(
                new Error("Conversation.Unauthorized", "Unauthorized access"));
        }

        var messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(_mapper.ToMessageResponse)
            .ToList();

        return messages;
    }
}
