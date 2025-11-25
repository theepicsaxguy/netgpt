// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
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
    public sealed class GetConversationsHandler(IConversationRepository repository, IConversationMapper mapper) : IRequestHandler<GetConversationsQuery, Result<PaginatedResponse<ConversationResponse>>>
    {
        private readonly IConversationRepository repository = repository;
        private readonly IConversationMapper mapper = mapper;

        public async Task<Result<PaginatedResponse<ConversationResponse>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
        {
            UserId userId = UserId.From(request.UserId);

            int skip = (request.Page - 1) * request.PageSize;
            List<Conversation> conversations = await repository.GetByUserIdAsync(userId, skip, request.PageSize, cancellationToken);
            int totalCount = await repository.CountByUserIdAsync(userId, cancellationToken);
            int totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            List<ConversationResponse> responses = [.. conversations.Select(mapper.ToResponse)];

            return new PaginatedResponse<ConversationResponse>(
                responses,
                request.Page,
                request.PageSize,
                totalCount,
                totalPages);
        }
    }
}
