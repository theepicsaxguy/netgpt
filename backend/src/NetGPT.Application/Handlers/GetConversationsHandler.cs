// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.Handlers
{
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

    public sealed class GetConversationsHandler(IConversationRepository repository, IConversationMapper mapper) : IRequestHandler<GetConversationsQuery, Result<PaginatedResponse<ConversationResponse>>>
    {
        private readonly IConversationRepository repository = repository;
        private readonly IConversationMapper mapper = mapper;

        public async Task<Result<PaginatedResponse<ConversationResponse>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
        {
            UserId userId = UserId.From(request.UserId);

            List<Conversation> conversations = await this.repository.GetByUserIdAsync(userId, request.Page, request.PageSize, cancellationToken);
            int totalCount = await this.repository.CountByUserIdAsync(userId, cancellationToken);
            int totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            List<ConversationResponse> responses = [.. conversations.Select(this.mapper.ToResponse)];

            return new PaginatedResponse<ConversationResponse>(
                responses,
                request.Page,
                request.PageSize,
                totalCount,
                totalPages);
        }
    }
}
