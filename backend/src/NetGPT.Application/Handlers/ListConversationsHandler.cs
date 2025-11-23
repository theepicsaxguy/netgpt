using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Queries;
using NetGPT.Domain.Interfaces;

namespace NetGPT.Application.Handlers;

public class ListConversationsHandler 
    : IRequestHandler<ListConversationsQuery, PaginatedResult<ConversationDto>>
{
    private readonly IConversationRepository _repository;

    public ListConversationsHandler(IConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<ConversationDto>> Handle(
        ListConversationsQuery request,
        CancellationToken ct)
    {
        var skip = (request.Page - 1) * request.PageSize;
        
        var conversations = await _repository.GetByUserIdAsync(
            request.UserId,
            skip,
            request.PageSize,
            ct
        );

        var totalCount = await _repository.CountByUserIdAsync(request.UserId, ct);

        var items = conversations.Select(c => new ConversationDto(
            c.Id.Value,
            c.Title,
            c.CreatedAt,
            c.UpdatedAt,
            c.Messages.Count
        )).ToList();

        return new PaginatedResult<ConversationDto>(
            items,
            totalCount,
            request.Page,
            request.PageSize
        );
    }
}