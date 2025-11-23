namespace NetGPT.Application.DTOs;

public record StreamingChunkDto(
    Guid MessageId,
    string? Content,
    ToolInvocationDto? ToolInvocation = null,
    bool IsComplete = false);