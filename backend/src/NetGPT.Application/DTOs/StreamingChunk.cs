
namespace NetGPT.Application.DTOs
{
    using System;

    public record StreamingChunkDto(
        Guid MessageId,
        string? Content,
        ToolInvocationDto? ToolInvocation = null,
        bool IsComplete = false);
}
