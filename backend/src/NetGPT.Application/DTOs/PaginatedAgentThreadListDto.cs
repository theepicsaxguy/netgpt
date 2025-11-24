// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;

namespace NetGPT.Application.DTOs
{
    public record PaginatedAgentThreadListDto(
        IEnumerable<AgentThreadSummaryDto> Items,
        int TotalCount,
        int Page,
        int PageSize);
}
