// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;

namespace NetGPT.Application.DTOs
{
    public record PaginatedResponse<T>(
        List<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);
}
