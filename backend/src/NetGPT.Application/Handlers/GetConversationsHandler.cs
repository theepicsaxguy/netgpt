using System;
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

public sealed class GetConversationsHandler : IRequestHandler<GetConversationsQuery, Result<PaginatedResponse<ConversationResponse>>>
{
    private readonly IConversationRepository _repository;
    private readonly IConversationMapper _mapper;

    public GetConversationsHandler(IConversationRepository repository, IConversationMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResponse<ConversationResponse>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.UserId);

        var conversations = await _repository.GetByUserIdAsync(userId, request.Page, request.PageSize, cancellationToken);
        var totalCount = await _repository.CountByUserIdAsync(userId, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var responses = conversations.Select(_mapper.ToResponse).ToList();

        return new PaginatedResponse<ConversationResponse>(
            responses,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}
