// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Queries;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Handlers
{
    public sealed class GetConversationHandler(IConversationRepository repository, IConversationMapper mapper) : IRequestHandler<GetConversationQuery, Result<ConversationResponse>>
    {
        private readonly IConversationRepository repository = repository;
        private readonly IConversationMapper mapper = mapper;

        public async Task<Result<ConversationResponse>> Handle(GetConversationQuery request, CancellationToken cancellationToken)
        {
            ConversationId conversationId = ConversationId.From(request.ConversationId);
            UserId userId = UserId.From(request.UserId);

            Conversation? conversation = await repository.GetByIdAsync(conversationId, cancellationToken);
            return conversation is null
                ? Result.Failure<ConversationResponse>(
                    new Error("Conversation.NotFound", "Conversation not found"))
                : conversation.UserId != userId
                ? Result.Failure<ConversationResponse>(
                    new Error("Conversation.Unauthorized", "Unauthorized access"))
                : (Result<ConversationResponse>)mapper.ToResponse(conversation);
        }
    }
}
