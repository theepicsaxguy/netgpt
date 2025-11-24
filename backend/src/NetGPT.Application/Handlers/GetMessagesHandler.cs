// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Linq;
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
    public sealed class GetMessagesHandler(IConversationRepository repository, IConversationMapper mapper) : IRequestHandler<GetMessagesQuery, Result<List<MessageResponse>>>
    {
        private readonly IConversationRepository repository = repository;
        private readonly IConversationMapper mapper = mapper;

        public async Task<Result<List<MessageResponse>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
        {
            ConversationId conversationId = ConversationId.From(request.ConversationId);
            UserId userId = UserId.From(request.UserId);

            Conversation? conversation = await repository.GetByIdAsync(conversationId, cancellationToken);
            if (conversation is null)
            {
                return Result.Failure<List<MessageResponse>>(
                    new DomainError("Conversation.NotFound", "Conversation not found"));
            }

            if (conversation.UserId != userId)
            {
                return Result.Failure<List<MessageResponse>>(
                    new DomainError("Conversation.Unauthorized", "Unauthorized access"));
            }

            List<MessageResponse> messages = [.. conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(mapper.ToMessageResponse)];

            return messages;
        }
    }
}
