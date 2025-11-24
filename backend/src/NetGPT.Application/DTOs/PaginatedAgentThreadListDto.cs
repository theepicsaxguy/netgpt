
namespace NetGPT.Application.DTOs
{
    using System;
    using System.Collections.Generic;

    public record AgentThreadSummaryDto(
        Guid Id,
        Guid ConversationId,
        string Status,
        DateTime StartedAt,
        DateTime? EndedAt);

    public record PaginatedAgentThreadListDto(
        IEnumerable<AgentThreadSummaryDto> Items,
        int TotalCount,
        int Page,
        int PageSize);
}
