
namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record PaginatedResponse<T>(
        List<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);
}
