using System.Collections.Generic;
using System.Linq;
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
        var conversationId = ConversationId.From(request.ConversationId);
        var userId = UserId.From(request.UserId);

        var conversation = await _repository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure<List<MessageResponse>>(
                new Error("Conversation.NotFound", "Conversation not found"));
        }

        if (conversation.UserId != userId)
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
