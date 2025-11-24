// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Queries;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Interfaces;

namespace NetGPT.Application.Handlers
{
    public class ListConversationsHandler(IConversationRepository repository)
            : IRequestHandler<ListConversationsQuery, PaginatedResult<ConversationDto>>
    {
        private readonly IConversationRepository repository = repository;

        public async Task<PaginatedResult<ConversationDto>> Handle(
            ListConversationsQuery request,
            CancellationToken cancellationToken)
        {
            int skip = (request.Page - 1) * request.PageSize;

            List<Conversation> conversations = await repository.GetByUserIdAsync(
                request.UserId,
                skip,
                request.PageSize,
                cancellationToken);

            int totalCount = await repository.CountByUserIdAsync(request.UserId, cancellationToken);

            List<ConversationDto> items = [.. conversations.Select(c => new ConversationDto(
                c.Id.Value,
                c.Title,
                c.CreatedAt,
                c.UpdatedAt,
                c.Messages.Count))];

            return new PaginatedResult<ConversationDto>(
                items,
                totalCount,
                request.Page,
                request.PageSize);
        }
    }
}
